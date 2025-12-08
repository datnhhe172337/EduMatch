using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TutorPayoutsController : ControllerBase
    {
        private readonly ITutorPayoutService _tutorPayoutService;

        public TutorPayoutsController(ITutorPayoutService tutorPayoutService)
        {
            _tutorPayoutService = tutorPayoutService;
        }

        /// <summary>
        /// Get tutor payouts by bookingId.
        /// </summary>
        [Authorize]
        [HttpGet("by-booking/{bookingId:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<TutorPayoutDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<TutorPayoutDto>>>> GetByBooking(int bookingId)
        {
            try
            {
                var payouts = await _tutorPayoutService.GetByBookingIdAsync(bookingId);
                return Ok(ApiResponse<List<TutorPayoutDto>>.Ok(payouts));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
