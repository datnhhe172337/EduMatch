
// Filename: WebhooksController.cs (REAL VERSION)
using EduMatch.BusinessLogicLayer.Interfaces;
using MyApiResponse = EduMatch.PresentationLayer.Common.ApiResponse<string>;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models;
using System;
using System.Threading.Tasks;
using PayOS.Models.Webhooks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IDepositService _depositService;

        public WebhooksController(IDepositService depositService)
        {
            _depositService = depositService;
        }

        // POST: api/webhooks/payos
        [HttpPost("payos")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MyApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> HandlePayosWebhook([FromBody] Webhook webhook)
        {
            // --- REAL CODE ---
            try
            {
                bool success = await _depositService.ProcessPayosWebhookAsync(webhook);
                return Ok();
            }
            catch (Exception ex)
            {
                // This will catch signature failures or database errors
                return BadRequest(MyApiResponse.Fail(ex.Message));
            }
            // -----------------
        }
    }
}