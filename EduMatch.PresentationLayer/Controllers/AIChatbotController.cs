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

        public AIChatbotController(IGeminiChatService gemini)
        {
            _gemini = gemini;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatAsync([FromBody] ChatRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            var response = await _gemini.GenerateTextAsync(req.Message);
            var resp = new ChatResponseDto { Reply = response };
            return Ok(resp);
        }
    }
}
