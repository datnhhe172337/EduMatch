using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduMatch.PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 All actions require an authenticated user
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated notifications for the current user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userEmail = GetCurrentUserEmail();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User is not authenticated."));

                var notifications = await _notificationService.GetNotificationsForUserAsync(userEmail, page, pageSize);
                return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notifications, "Notifications retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for user.");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while retrieving notifications.", ex.Message));
            }
        }

        /// <summary>
        /// Get the count of unread notifications for the current user.
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userEmail = GetCurrentUserEmail();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User is not authenticated."));

                var count = await _notificationService.GetUnreadNotificationCountAsync(userEmail);
                return Ok(ApiResponse<object>.Ok(new { unreadCount = count }, "Unread count retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread notification count.");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while retrieving unread count.", ex.Message));
            }
        }

        /// <summary>
        /// Mark a specific notification as read.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        [HttpPost("{id:int}/read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userEmail = GetCurrentUserEmail();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User is not authenticated."));

                var success = await _notificationService.MarkAsReadAsync(id, userEmail);

                if (!success)
                    return NotFound(ApiResponse<string>.Fail("Notification not found or already read."));

                return Ok(ApiResponse<object>.Ok(null, "Notification marked as read."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read.", id);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while marking as read.", ex.Message));
            }
        }

        /// <summary>
        /// Mark all unread notifications as read.
        /// </summary>
        [HttpPost("read-all")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userEmail = GetCurrentUserEmail();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User is not authenticated."));

                await _notificationService.MarkAllAsReadAsync(userEmail);
                return Ok(ApiResponse<object>.Ok(null, "All notifications marked as read."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user.");
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while marking all as read.", ex.Message));
            }
        }

        /// <summary>
        /// Delete a specific notification.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var userEmail = GetCurrentUserEmail();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User is not authenticated."));

                var success = await _notificationService.DeleteNotificationAsync(id, userEmail);

                if (!success)
                    return NotFound(ApiResponse<string>.Fail("Notification not found."));

                return Ok(ApiResponse<object>.Ok(null, "Notification deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}.", id);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while deleting the notification.", ex.Message));
            }
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
