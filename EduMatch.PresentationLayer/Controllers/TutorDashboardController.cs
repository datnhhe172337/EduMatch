using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/tutor-dashboard")]
    [ApiController]
    [Authorize(Roles = Roles.Tutor)]
    public class TutorDashboardController : ControllerBase
    {
        private readonly ITutorDashboardService _dashboardService;
        private readonly CurrentUserService _currentUserService;

        public TutorDashboardController(ITutorDashboardService dashboardService, CurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet("upcoming-lessons")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ScheduleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpcomingLessonsAsync()
        {
            var email = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var data = await _dashboardService.GetUpcomingLessonsAsync(email);
            return Ok(ApiResponse<IReadOnlyList<ScheduleDto>>.Ok(data, "Upcoming lessons retrieved successfully."));
        }

        [HttpGet("today-schedules")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ScheduleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodaySchedulesAsync()
        {
            var email = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var data = await _dashboardService.GetTodaySchedulesAsync(email);
            return Ok(ApiResponse<IReadOnlyList<ScheduleDto>>.Ok(data, "Today's schedules retrieved successfully."));
        }

        [HttpGet("pending-bookings")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BookingDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingBookingsAsync()
        {
            var email = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var data = await _dashboardService.GetPendingBookingsAsync(email);
            return Ok(ApiResponse<IReadOnlyList<BookingDto>>.Ok(data, "Pending bookings retrieved successfully."));
        }

        [HttpGet("earnings/monthly")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TutorMonthlyEarningDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMonthlyEarningsAsync([FromQuery] int year)
        {
            if (year <= 0)
                return BadRequest(ApiResponse<string>.Fail("Year must be a positive integer."));

            var email = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var data = await _dashboardService.GetMonthlyEarningsAsync(email, year);
            return Ok(ApiResponse<IReadOnlyList<TutorMonthlyEarningDto>>.Ok(data, "Monthly earnings retrieved successfully."));
        }

        [HttpGet("earnings/current-month")]
        [ProducesResponseType(typeof(ApiResponse<TutorMonthlyEarningDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentMonthEarningAsync()
        {
            var email = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var data = await _dashboardService.GetCurrentMonthEarningAsync(email);
            return Ok(ApiResponse<TutorMonthlyEarningDto>.Ok(data, "Current month earnings retrieved successfully."));
        }

        [HttpGet("reports/pending-defense")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReportListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReportsPendingDefenseAsync()
        {
            var email = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var data = await _dashboardService.GetReportsPendingDefenseAsync(email);
            return Ok(ApiResponse<IReadOnlyList<ReportListItemDto>>.Ok(data, "Reports pending defense retrieved successfully."));
        }
    }
}
