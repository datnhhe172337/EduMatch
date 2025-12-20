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
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Crmf;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

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

                var systemPrompt = _promptService.PromptV4();

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
                var vectorResults = await _qdrantService.SearchTutorsAsync(embeddingVector, topK: 5);

                // Step 3: Keyword search
                var keywordResults = await _service.SearchByKeywordAsync(req.Message);

                //Step 4: Merge & Rank
                var final = await _qdrantService.MergeAndRankAsync(vectorResults, keywordResults);

                //Step 5: Rerank
                float threshold = 0.6f;
                var filteredTutors = final
                    .Where(t => t.Score >= threshold)
                    .OrderByDescending(t => t.Score)
                    .Take(3)
                    .ToList();

                if(filteredTutors == null || !filteredTutors.Any())
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
                // Step 6: Build Context + Prompt
                var contextJson = BuildContextJson(filteredTutors);
                var contextJsonString = JsonSerializer.Serialize(contextJson, new JsonSerializerOptions { WriteIndented = false });

                 Console.WriteLine(contextJsonString);

                var prompt = $@" 

                    Người dùng hỏi: ""{req.Message}""
                    
                    Dưới đây là danh sách tutor phù hợp (JSON context):
                    {contextJsonString}

                    Hãy trả lời **chỉ duy nhất JSON** theo schema sau:

                    {{
                      ""message"": ""string"",
                      ""tutors"": [
                        {{
                          ""rank"": integer,
                          ""tutorId"": integer,
                          ""name"": ""string"",
                          ""subjects"": [""string""],
                          ""levels"": [""string""],
                          ""province"": ""string"",
                          ""subDistrict"": ""string"",
                          ""hourlyRates"": [number],
                          ""teachingExp"": ""string"",
                          ""profileUrl"": ""string"",
                          ""matchScore"": number
                        }}
                      ]
                    }}

                  {systemPrompt}

                ";


                // Step 4: Call LLM - Gemini
                var response = await _gemini.GenerateTextAsync(sessionId, prompt, req.Message);

                //response = CleanLLMJson(response);

                var cleanJson = CleanLLMJson(response);
                var llmResult = JsonSerializer.Deserialize<SuggestionsDto>(cleanJson);
                //try
                //{
                //    llmResult = JsonSerializer.Deserialize<SuggestionsDto>(cleanJson)
                //                ?? new SuggestionsDto { Message = "Không tìm thấy tutor phù hợp" };
                //}
                //catch
                //{
                //    llmResult = new SuggestionsDto { Message = "Không tìm thấy tutor phù hợp" };
                //}

                return Ok(new ChatResponseDto
                {
                    SessionId = sessionId,
                    Reply = llmResult.Message,
                    Suggestions = llmResult.Tutors,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        private string CleanLLMJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "{}";

            int start = input.IndexOf('{');
            int end = input.LastIndexOf('}');

            if (start < 0 || end < 0 || end <= start) return "{}";

            string json = input[start..(end + 1)];
            return json;
        }

        private object BuildContextJson(List<(TutorProfileDto Tutor, float Score)> topTutors)
        {
            //if (topTutors == null || !topTutors.Any())
            //    return new { message = "Không tìm thấy tutor phù hợp.", tutors = new List<object>() };

            var tutorList = topTutors.Select((t, idx) =>
            {
                var tutor = t.Tutor;
                var tutorSubjects = tutor.TutorSubjects ?? new List<TutorSubjectDto>();

                var subjects = tutorSubjects
                    .Select(s => s.Subject.SubjectName)
                    .Distinct()
                    .ToList();

                var levelNumbers = tutorSubjects
                    .Select(s => s.Level.Name) 
                    .Distinct()
                    .ToList();

                var hourlyRates = tutorSubjects
                    .Select(s => s.HourlyRate)
                    .Distinct()
                    .ToList();

                return new
                {
                    rank = idx + 1,
                    tutorId = tutor.Id,
                    name = tutor.UserName,

                    subjects = subjects,
                    levels = levelNumbers,
                    hourlyRate = hourlyRates,

                    province = tutor.Province?.Name,
                    subDistrict = tutor.SubDistrict?.Name,
                    teachingExp = tutor.TeachingExp,
                    matchScore = Math.Round(t.Score, 2),
                    profileUrl = $"http://localhost:3000/tutor/{tutor.Id}"
                };
            }).ToList();

            return new
            {
                message = $"Tìm thấy {tutorList.Count} gia sư phù hợp với yêu cầu của bạn",
                tutors = tutorList
            };
        }

        //private static string NormalizeLevels(List<string> levels)
        //{
        //    var distinct = levels.Distinct().OrderBy(x => x).ToList();

        //    if (!distinct.Any()) return "";

        //    if (distinct.Count > 1 &&
        //        distinct.Last() - distinct.First() + 1 == distinct.Count)
        //    {
        //        return $"Lớp {distinct.First()}–{distinct.Last()}";
        //    }

        //    return string.Join(", ", distinct.Select(l => $"Lớp {l}"));
        //}

        //private static string NormalizeHourlyRate(List<decimal?> rates)
        //{
        //    var distinct = rates.Distinct().OrderBy(x => x).ToList();

        //    if (distinct.Count == 1)
        //        return $"{distinct[0]:N0}đ / giờ";

        //    return $"{distinct.First():N0}đ – {distinct.Last():N0}đ / giờ";
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
