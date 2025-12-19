using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using MyApiResponse = EduMatch.PresentationLayer.Common.ApiResponse<string>;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IDepositService _depositService;
        private readonly IVnpayService _vnpayService; 

        public WebhooksController(
            IDepositService depositService,
            IVnpayService vnpayService)
        {
            _depositService = depositService;
            _vnpayService = vnpayService;
        }
    
        [HttpGet("vnpay-ipn")] // VNPay sends confirmation via GET
        [AllowAnonymous]
        public async Task<IActionResult> HandleVnpayIpn()
        {
            try
            {
                // 1. Validate the response from VNPay
                var response = _vnpayService.ValidatePaymentResponse(Request.Query);

                if (response.IsSuccess && response.ResponseCode == "00")
                {
                    // 2. Signature is valid and payment was successful
                    // Process the payment (This is your database logic)
                    int depositId = int.Parse(response.OrderId);
                    await _depositService.ProcessVnpayPaymentAsync(depositId, response.TransactionId, response.Amount);

                    // --- MUST Return this JSON to VNPay to confirm ---
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    // Signature was invalid or payment failed
                    return Ok(new { RspCode = "97", Message = "Invalid Signature or Failed Payment" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { RspCode = "99", Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}