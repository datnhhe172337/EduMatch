using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IHybridSearchService _hybrid;
        private readonly ILLMRerankService _rerank;
        private readonly HybridOptions _opts;

        public AIChatbotController(IGeminiChatService gemini, IEmbeddingService embedding, IHybridSearchService hybrid, ILLMRerankService rerank, IOptions<HybridOptions> opts)
        {
            _gemini = gemini;
            _embedding = embedding;
            _hybrid = hybrid;
            _rerank = rerank;
            _opts = opts.Value;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatAsync([FromBody] ChatRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            // Step 1: Embedding
            var embeddingVector = await _embedding.GenerateEmbeddingAsync(req.Message);

            if (embeddingVector == null || embeddingVector.Length != 768)
                throw new InvalidOperationException("Embedding vector is null or has invalid length.");
            // Step 2: Hybrid Search (BM25 + Vector)
            var hybridHits = await _hybrid.SearchAsync(req.Message, embeddingVector, Math.Max(_opts.RerankTopN, _opts.ReturnTopK));
            // Step 3: LLM Rerank- Gemini
            var reranked = await _rerank.RerankAsync(req.Message, hybridHits);

            var map = hybridHits.ToDictionary(h => h.TutorId);
            var final = reranked
                .Where(r => map.ContainsKey(r.TutorId))
                .Select(r =>
                {
                    var h = map[r.TutorId];
                    h.Score = r.FinalScore;
                    return h;
                })
                .OrderByDescending(h => h.Score)
                .Take(_opts.ReturnTopK)
                .ToList();

            // fallback if LLM returned empty
            if (final.Count == 0)
            {
                final = hybridHits.OrderByDescending(h => h.Score).Take(_opts.ReturnTopK).ToList();
            }
            // Step 4: Buld Context + Prompt
            var contextText = string.Join("\n", reranked.Select((t, i) =>
                $"{i + 1}. {t.TutorId} (Score: {t.FinalScore:F2})"));

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
