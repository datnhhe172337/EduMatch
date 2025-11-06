using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduMatch.PresentationLayer.Common; 
using EduMatch.BusinessLogicLayer.Services;  
using EduMatch.BusinessLogicLayer.DTOs;    
using Microsoft.AspNetCore.Http;
using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteTutorController : ControllerBase
    {
        private readonly IFavoriteTutorService _favoriteTutorService;
        private readonly CurrentUserService _currentUserService; 

        public FavoriteTutorController(
            IFavoriteTutorService favoriteTutorService,
            CurrentUserService currentUserService) 
        {
            _favoriteTutorService = favoriteTutorService;
            _currentUserService = currentUserService;
        }


        [HttpPost("add/{tutorId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddToFavorite(int tutorId)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                bool result = await _favoriteTutorService.AddToFavoriteAsync(userEmail, tutorId);

                if (!result)
                    return BadRequest(ApiResponse<string>.Fail("Tutor is already in favorites."));

                return Ok(ApiResponse<string>.Ok("Tutor added to favorites successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to add favorite.", ex.Message));
            }
        }

        [HttpDelete("remove/{tutorId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFromFavorite(int tutorId)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                bool result = await _favoriteTutorService.RemoveFromFavoriteAsync(userEmail, tutorId);

                if (!result)
                    return NotFound(ApiResponse<string>.Fail("Tutor not found in your favorites."));

                return Ok(ApiResponse<string>.Ok("Tutor removed from favorites."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to remove favorite.", ex.Message));
            }
        }

        // 🔍 GET: api/FavoriteTutor/is-favorite/5
        [HttpGet("is-favorite/{tutorId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> IsFavorite(int tutorId)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                bool result = await _favoriteTutorService.IsFavoriteAsync(userEmail, tutorId);
                return Ok(ApiResponse<bool>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to check favorite status.", ex.Message));
            }
        }

        // 📜 GET: api/FavoriteTutor/list
        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<List<TutorProfileDto>>), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFavoriteTutors()
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var tutors = await _favoriteTutorService.GetFavoriteTutorsAsync(userEmail); 
                return Ok(ApiResponse<List<TutorProfile>>.Ok(tutors));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to get favorite tutors.", ex.Message));
            }
        }
    }
}