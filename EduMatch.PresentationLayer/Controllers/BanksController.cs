using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Exposes read-only bank metadata for populating drop-downs.
    /// </summary>
    public class BanksController : ControllerBase
    {
        private readonly IBankService _bankService;

        public BanksController(IBankService bankService)
        {
            _bankService = bankService;
        }

        /// <summary>
        /// Lists all supported banks.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BankDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllBanks()
        {
            try
            {
                var banks = await _bankService.GetAllBanksAsync();
                return Ok(ApiResponse<IEnumerable<BankDto>>.Ok(banks));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to get bank list.", ex.Message));
            }
        }
    }
}
