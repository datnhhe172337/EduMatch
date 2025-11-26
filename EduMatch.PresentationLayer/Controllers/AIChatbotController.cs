using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Crmf;
using System.Security.Claims;
using System.Text.Json;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatbotController : ControllerBase
    {
        private readonly IGeminiChatService _gemini;
        private readonly IEmbeddingService _embedding;
        private readonly ITutorProfileService _service;
        private readonly IQdrantService _qdrantService;
        private readonly IChatbotService _chatbotService;
        private readonly IPromptService _promptService;

        public AIChatbotController(IGeminiChatService gemini, IEmbeddingService embedding, ITutorProfileService service, IQdrantService qdrantService, IChatbotService chatbotService, IPromptService promptService)
        {
            _gemini = gemini;
            _embedding = embedding;
            _service = service;
            _qdrantService = qdrantService;
            _chatbotService = chatbotService;
            _promptService = promptService;
        }

        [HttpPost("session")]
        public async Task<IActionResult> CreateChatSession()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var sessionId = await _chatbotService.CreateSessionAsync(userEmail);
                return Ok(new { sessionId = sessionId });
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [Authorize(Roles = "Learner")]
        [HttpGet("listSessionByUserEmail")]
        public async Task<IActionResult> GetListSessions()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { Message = "User authentication failed." });

            try
            {
                var sessionsByUserEmail = await _chatbotService.GetListSessionsByUserEmail(userEmail);

                var sessions = sessionsByUserEmail.Select(s => new
                {
                    s.Id,
                    s.CreatedAt,
                    LastMessage = s.ChatbotMessages
                            .OrderByDescending(m => m.CreatedAt)
                            .Select(m => m.Message)
                            .FirstOrDefault()
                });

                return Ok(sessions);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }

        /// <summary>
        /// Lịch sử chat của từng session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>

        [Authorize(Roles = "Learner")]
        [HttpGet("chatHistory")]
        public async Task<IActionResult> GetChatHistory(int sessionId)
        {
            var session = await _chatbotService.GetMessagesHistoryAsync(sessionId);

            var messages = session.Select(m => new {
                m.SessionId,
                m.Role,
                m.Message,
                m.CreatedAt
            });

            return Ok(messages);
        }

        [Authorize(Roles = "Learner")]
        [HttpDelete("session/{sessionId}")]
        public async Task<IActionResult> DeleteSessionAsync(int sessionId)
        {
            var deleted = await _chatbotService.DeleteSessionAsync(sessionId);

            if (!deleted)
                return NotFound(new { message = "Session not found" });

            return Ok(new { message = "Session deleted successfully" });
        }



        [HttpPost("chat")]
        public async Task<IActionResult> ChatAsync([FromBody] ChatRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            int sessionId = req.SessionId ?? await _chatbotService.CreateSessionAsync(userEmail);

            try
            {
                // Step 1: Embedding
                var embeddingVector = await _embedding.GenerateEmbeddingAsync(req.Message);

                if (embeddingVector == null || embeddingVector.Length != 768)
                    throw new InvalidOperationException("Embedding vector is null or has invalid length.");

                var systemPrompt = _promptService.PromptV1(); ;

                if (_chatbotService.IsTutorQuery(req.Message) == false)
                {

                    var promptWithQueryIsNull = $@"
                    Người dùng hỏi: ""{req.Message}""

                    Hãy trả lời người dùng theo hướng dẫn như sau: 
                    {systemPrompt}
                    ";

                    var resp = await _gemini.GenerateTextAsync(sessionId, promptWithQueryIsNull, req.Message);
                    return Ok(new ChatResponseDto
                    {
                        SessionId = sessionId,
                        Reply = resp
                    });

                }

                // Step 2: Vector search (Semantic search) - Qdrant
                var topTutors = await _qdrantService.SearchTutorsAsync(embeddingVector, topK: 5);

                // Filter theo score threshold
                float threshold = 0.65f;
                var filteredTutors = topTutors
                    .Where(t => t.Score >= threshold)
                    .OrderByDescending(t => t.Score)
                    .Take(3)
                    .ToList();

                if (!filteredTutors.Any())
                {
                    var promptWithQueryIsNull = $@"
                    Người dùng hỏi: ""{req.Message}""
                    
                    Không có tutor nào phù hợp từ yêu cầu của người dùng.

                    Hãy trả lời người dùng theo hướng dẫn như sau: 
                    {systemPrompt}
                    ";

                    var resp = await _gemini.GenerateTextAsync(sessionId, promptWithQueryIsNull, req.Message);

                    return Ok(new ChatResponseDto
                    {
                        SessionId = sessionId,
                        Reply = resp
                    });
                }

                // Step 3: Buld Context + Prompt
                var contextJson = BuildContextJson(filteredTutors);
                var contextJsonString = JsonSerializer.Serialize(contextJson);

                var prompt = $@"
                    Người dùng hỏi: ""{req.Message}""
                 
                    Dưới đây là danh sách tutor phù hợp (JSON context):
                    {contextJsonString}

                    Hãy trả lời người dùng theo hướng dẫn như sau: 
                    {systemPrompt}
                    ";


                // Step 4: Call LLM - Gemini
                var response = await _gemini.GenerateTextAsync(sessionId, prompt, req.Message);

                return Ok(new ChatResponseDto
                {
                    SessionId = sessionId,
                    Reply = response,
                    Suggestions = contextJson
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }


        private object BuildContextJson(List<(TutorProfileDto Tutor, float Score)> topTutors)
        {
            if (topTutors == null || !topTutors.Any())
                return new { message = "Không tìm thấy tutor phù hợp.", tutors = new List<object>() };

            var tutorList = topTutors.Select((t, idx) =>
            {
                var tutor = t.Tutor;
                var subjects = tutor.TutorSubjects?.Select(s => s.Subject.SubjectName).ToList() ?? new List<string>();
                var levels = tutor.TutorSubjects?.Select(s => s.Level.Name).ToList() ?? new List<string>();
                var hourlyRates = tutor.TutorSubjects?.Select(s => s.HourlyRate).ToList();

                return new
                {
                    Rank = idx + 1,
                    TutorId = tutor.Id,
                    Name = tutor.UserName,
                    Subjects = subjects,
                    Levels = levels,
                    TeachingExp = tutor.TeachingExp,
                    Province = tutor.Province?.Name,
                    SubDistrict = tutor.SubDistrict?.Name,
                    HourlyRates = hourlyRates.Select(r => $"{r}").ToList(),
                    MatchScore = Math.Round(t.Score, 2),
                    ProfileUrl = $"http://localhost:3000/tutor/{tutor.Id}"
                };
            }).ToList();

            return new
            {
                message = $"Tìm thấy {tutorList.Count} tutor phù hợp",
                tutors = tutorList
            };
        }

        //[HttpPost("testCallLLM")]
        //public async Task<IActionResult> TestCallLLMAsync([FromBody] ChatRequestDto req)
        //{
        //    if (string.IsNullOrWhiteSpace(req.Message))
        //        return BadRequest("Message is required");

        //    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        //    int sessionId = req.SessionId ?? await _chatbotService.CreateSessionAsync(userEmail);

        //    try
        //    {
        //        // Step 5: Call LLM - Gemini
        //        var response = await _gemini.GenerateTextAsync(sessionId, req.Message);

        //        var resp = new ChatResponseDto { Reply = response };
        //        return Ok(resp);
        //    }
        //    catch (InvalidOperationException ex) { throw new Exception(ex.Message); }

        //}


        /// <summary>
        /// Đồng bộ toàn bộ tutor trong DB lên Qdrant -> Đừng chạy hàm này
        /// </summary>
        [HttpPost("sync-tutors")]
        public async Task<IActionResult> SyncAllTutors()
        {
            await _qdrantService.CreateCollectionAsync("tutors", 768);

            var count = await _service.SyncAllTutorsAsync();

            return Ok(new
            {
                message = "Sync completed",
                tutorsSynced = count,
                timestamp = DateTime.UtcNow
            });
        }



    }
}
