using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        //[HttpGet("signups/trend")]
        //[ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SignupTrendPointDto>>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetSignupTrendAsync([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string groupBy = "day")
        //{
        //    var data = await _adminStatsService.GetSignupTrendAsync(from, to, groupBy);
        //    return Ok(ApiResponse<IReadOnlyList<SignupTrendPointDto>>.Ok(data, "Signup trend retrieved successfully."));
        //}

        //[HttpGet("bookings/trend")]
        //[ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BookingTrendPointDto>>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetBookingTrendAsync([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string groupBy = "day")
        //{
        //    var data = await _adminStatsService.GetBookingTrendAsync(from, to, groupBy);
        //    return Ok(ApiResponse<IReadOnlyList<BookingTrendPointDto>>.Ok(data, "Booking trend retrieved successfully."));
        //}

        [HttpGet("monthly")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MonthlyAdminStatsDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMonthlyStatsAsync([FromQuery] int year)
        {
            if (year <= 0)
                return BadRequest(ApiResponse<string>.Fail("Year must be a positive integer."));

            var data = await _adminStatsService.GetMonthlyStatsAsync(year);
            return Ok(ApiResponse<IReadOnlyList<MonthlyAdminStatsDto>>.Ok(data, "Monthly admin stats retrieved successfully."));
        }
    }
}
