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

        public AIChatbotController(IGeminiChatService gemini, IEmbeddingService embedding, ITutorProfileService service, IQdrantService qdrantService, IChatbotService chatbotService)
        {
            _gemini = gemini;
            _embedding = embedding;
            _service = service;
            _qdrantService = qdrantService;
            _chatbotService = chatbotService;
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

                // Step 2: Vector search (Semantic search) - Qdrant
                var topTutors = await _qdrantService.SearchTutorsAsync(embeddingVector, topK: 3);

                // Step 3: Buld Context + Prompt
                var contextText = BuildContextText(topTutors);

                var systemPrompt = PromptV1();
                var prompt = $@"
                    Người dùng hỏi: ""{req.Message}""
                 
                    Dưới đây là danh sách tutor phù hợp (đã được hệ thống sắp xếp theo mức độ phù hợp):
                    {contextText}

                    Hãy trả lời người dùng theo hướng dẫn như sau: 
                    {systemPrompt}
                    ";


                // Step 4: Call LLM - Gemini
                var response = await _gemini.GenerateTextAsync(sessionId, prompt, req.Message);

                return Ok(new ChatResponseDto
                {
                    SessionId = sessionId,   // trả về để client lưu lại
                    Reply = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        private string BuildContextText(List<(TutorProfileDto Tutor, float Score)> topTutors)
        {
            return string.Join("\n\n", topTutors.Select((t, idx) =>
            {
                var tutor = t.Tutor;
                var subjects = string.Join(", ",
                    tutor.TutorSubjects?.Select(s => s.Subject.SubjectName)
                    ?? new List<string>());

                var levels = string.Join(", ",
                    tutor.TutorSubjects?.Select(s => s.Level.Name)
                    ?? new List<string>());

                var hourlyRates = string.Join(", ",
                    tutor.TutorSubjects?.Select(s => s.HourlyRate + "k/h")
                    ?? new List<string>());

                return $@"{idx + 1}. **{tutor.UserName}**
                    • **Môn dạy:** {subjects}
                    • **Lớp:** {levels}
                    • **Học phí:** {hourlyRates}
                    • **Kinh nghiệm:** {tutor.TeachingExp ?? "Không rõ"}
                    • **Khu vực:** {tutor.Province?.Name ?? "N/A"} - {tutor.SubDistrict?.Name ?? "N/A"}
                    • **Độ phù hợp:** {t.Score:F2}
                    • **Hồ sơ gia sư:** http://localhost:3000/tutor/{tutor.Id}";
            }));
        }

        private string PromptV1()
        {
            string prompt = @"
                    Bạn là EduMatch AI – trợ lý ảo hỗ trợ người học tìm kiếm gia sư.

                    Nhiệm vụ của bạn:
                    1. Phân tích câu hỏi của người dùng và hiểu nhu cầu thật sự: môn học, lớp, khu vực, giới tính gia sư, ngân sách, yêu cầu đặc biệt.
                    2. Dựa trên danh sách tutor đã được sắp xếp theo mức độ phù hợp (đã gửi kèm), hãy:
                       - Giới thiệu những gia sư phù hợp nhất.
                       - Trình bày thân thiện, dễ hiểu, không quá dài.
                       - Đính kèm link hồ sơ của từng gia sư để người dùng xem chi tiết.
                    3. Nếu danh sách gia sư trống, hãy hướng dẫn người dùng mô tả rõ nhu cầu hơn.
                    4. Nếu người dùng hỏi nội dung *không liên quan* đến tìm gia sư (ví dụ: hỏi kiến thức, hỏi đời tư, hỏi triết lý, chém gió…):
                       - Không từ chối thẳng thừng.
                       - Hãy trả lời ngắn gọn, lịch sự, và khéo léo hướng họ quay lại chủ đề tìm gia sư.
                       - Nhắc nhẹ rằng bạn được thiết kế chủ yếu để hỗ trợ tìm gia sư (ví dụ: “Nếu bạn cần tìm gia sư, mình luôn sẵn sàng hỗ trợ”).
                    5. Không trả về JSON hoặc dữ liệu kỹ thuật, chỉ trả lời dạng văn bản tự nhiên.
                    ";
            return prompt;
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





    }
}
