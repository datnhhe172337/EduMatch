using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Crmf;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatbotController : ControllerBase
    {
        private readonly IGeminiChatService _gemini;
        private readonly IEmbeddingService _embedding;
        private readonly IHybridSearchService _hybrid;
        private readonly ILLMRerankService _rerank;

        public AIChatbotController(IGeminiChatService gemini, IEmbeddingService embedding, IHybridSearchService hybrid, ILLMRerankService rerank)
        {
            _gemini = gemini;
            _embedding = embedding;
            _hybrid = hybrid;
            _rerank = rerank;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatAsync([FromBody] ChatRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            // Step 1: Embedding
            var embeddingVector = await _embedding.GenerateEmbeddingAsync(req.Message);
            // Step 2: Hybrid Search (BM25 + Vector)
            var hybridHits = await _hybrid.SearchAsync(req.Message, embeddingVector, 10);
            // Step 3: LLM Rerank- Gemini
            var reRanked = await _rerank.RerankAsync(req.Message, hybridHits);
            // Step 4: Buld Context + Prompt
            var contextText = string.Join("\n", reRanked.Select((t, i) =>
                $"{i + 1}. {t.TutorInfo} (Score: {t.Score:F2})"));

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
    }
}
