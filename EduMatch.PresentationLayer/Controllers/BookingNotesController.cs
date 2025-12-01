using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.BookingNote;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
    public class BookingNotesController : ControllerBase
    {
        private readonly IBookingNoteService _bookingNoteService;

        public BookingNotesController(IBookingNoteService bookingNoteService)
        {
            _bookingNoteService = bookingNoteService;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<BookingNoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var note = await _bookingNoteService.GetByIdAsync(id);
                if (note == null)
                    return NotFound(ApiResponse<object>.Fail("Booking note not found."));

                return Ok(ApiResponse<BookingNoteDto>.Ok(note));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("by-booking/{bookingId:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<BookingNoteDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            try
            {
                var notes = await _bookingNoteService.GetByBookingIdAsync(bookingId);
                return Ok(ApiResponse<List<BookingNoteDto>>.Ok(notes));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BookingNoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] BookingNoteCreateRequest request)
        {
            try
            {
                var note = await _bookingNoteService.CreateAsync(request);
                return Ok(ApiResponse<BookingNoteDto>.Ok(note, "Booking note created."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<BookingNoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] BookingNoteUpdateRequest request)
        {
            try
            {
                request.Id = id;
                var updated = await _bookingNoteService.UpdateAsync(request);
                if (updated == null)
                    return NotFound(ApiResponse<object>.Fail("Booking note not found."));

                return Ok(ApiResponse<BookingNoteDto>.Ok(updated, "Booking note updated."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _bookingNoteService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<object>.Fail("Booking note not found."));

                return Ok(ApiResponse<string>.Ok("Booking note deleted."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
