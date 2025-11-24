using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/admin-stats")]
    [ApiController]
    [Authorize(Roles = Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
    public class AdminStatsController : ControllerBase
    {
        private readonly IAdminStatsService _adminStatsService;

        public AdminStatsController(IAdminStatsService adminStatsService)
        {
            _adminStatsService = adminStatsService;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AdminSummaryStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummaryAsync()
        {
            var data = await _adminStatsService.GetSummaryAsync();
            return Ok(ApiResponse<AdminSummaryStatsDto>.Ok(data, "Admin summary stats retrieved successfully."));
        }
    }
}
