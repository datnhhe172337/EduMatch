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

        public AIChatbotController(IGeminiChatService gemini, IEmbeddingService embedding)
        {
            _gemini = gemini;
            _embedding = embedding;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatAsync([FromBody] ChatRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            // Step 2: Embedding
            var vector = await _embedding.GenerateEmbeddingAsync(req.Message);
            // Step 3: Vector DB search (top-k)


            var response = await _gemini.GenerateTextAsync(req.Message);
            var resp = new ChatResponseDto { Reply = response };
            return Ok(resp);
        }
    }
}
