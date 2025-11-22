using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Crmf;

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

        public AIChatbotController(IGeminiChatService gemini, IEmbeddingService embedding, ITutorProfileService service, IQdrantService qdrantService)
        {
            _gemini = gemini;
            _embedding = embedding;
            _service = service;
            _qdrantService = qdrantService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatAsync([FromBody] ChatRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            try
            {
                // Step 1: Embedding
                var embeddingVector = await _embedding.GenerateEmbeddingAsync(req.Message);

                //if (embeddingVector == null || embeddingVector.Length != 768)
                //    throw new InvalidOperationException("Embedding vector is null or has invalid length.");

                // Step 2: Vector search (Semantic search) - Qdrant
                var topTutors = await _qdrantService.SearchTutorsAsync(embeddingVector, topK: 5);
                // Step 3: LLM Rerank- Gemini

                // Step 4: Buld Context + Prompt
                var contextText = string.Join("\n", topTutors.Select((t, idx) =>
            $"{idx + 1}. {t.Tutor.UserName} - Môn: {string.Join(", ", t.Tutor.TutorSubjects.Select(s => s.Subject.SubjectName))} - Kinh nghiệm: {t.Tutor.TeachingExp} - Score: {t.Score:F2}"
                ));

                var prompt = $@"
                Bạn là một chatbot hỗ trợ người học tìm gia sư.
                Người dùng hỏi: ""{req.Message}""
                Danh sách tutor đã sắp xếp theo mức độ phù hợp:
                {contextText}

                Hãy trả lời người dùng dạng tự nhiên, gợi ý tutor phù hợp và nêu thông tin ngắn gọn. 
                Chỉ trả lời bằng văn bản, không JSON.
                ";

                // Step 5: Call LLM - Gemini
                var response = await _gemini.GenerateTextAsync(prompt);

                var resp = new ChatResponseDto { Reply = response };
                return Ok(resp);
            } 
            catch(InvalidOperationException ex) { throw new Exception(ex.Message); }
            
        }

        [HttpPost("testCallLLM")]
        public async Task<IActionResult> TestCallLLMAsync([FromBody] ChatRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            try
            {
                // Step 5: Call LLM - Gemini
                var response = await _gemini.GenerateTextAsync(req.Message);

                var resp = new ChatResponseDto { Reply = response };
                return Ok(resp);
            }
            catch (InvalidOperationException ex) { throw new Exception(ex.Message); }

        }


        /// <summary>
        /// Đồng bộ toàn bộ tutor trong DB lên Qdrant
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
