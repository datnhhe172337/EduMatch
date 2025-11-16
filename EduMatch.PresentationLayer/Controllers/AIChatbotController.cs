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

                if (embeddingVector == null || embeddingVector.Length != 768)
                    throw new InvalidOperationException("Embedding vector is null or has invalid length.");

                // Step 2: Vector search (Semantic search) - Qdrant

                // Step 3: LLM Rerank- Gemini

                // Step 4: Buld Context + Prompt
                var contextText = "";

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

        [HttpPost("sync-tutors")]
        public async Task<IActionResult> SyncAllTutors()
        {
            var tutors = await _service.GetAllFullAsync(); // Lấy tất cả tutor từ DB

            await _qdrantService.UpsertTutorsAsync(tutors);

            foreach (var tutor in tutors)
            {
                tutor. = DateTime.UtcNow; // cập nhật LastSync
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "All tutors synced to Qdrant successfully." });
        }


    }
}
