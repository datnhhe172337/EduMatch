using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Requests.GoogleMeeting;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GoogleAuthController : ControllerBase
	{
		private readonly IGoogleAuthService _googleAuthService;
		private readonly IGoogleCalendarService _googleCalendarService;
		public GoogleAuthController(IGoogleAuthService googleAuthService, IGoogleCalendarService googleCalendarService)
		{
			_googleAuthService = googleAuthService;
			_googleCalendarService = googleCalendarService;
		}

		/// <summary>
		/// Tạo URL ủy quyền Google OAuth2 cho tài khoản hệ thống (system account) FE không cần quan tâm
		/// </summary>

	[HttpGet("authorize")]
	public async Task<IActionResult> Authorize()
	{
		string result = await _googleAuthService.GenerateAuthUrlDat();
		
		// Kiểm tra xem result là URL hay message
		// URL thường bắt đầu bằng "https://"
		if (result.StartsWith("https://"))
		{
			// Trả về URL khi cần reauth
			return Ok(new { authUrl = result, needsAuth = true });
		}
		else
		{
			// Trả về message khi refresh token còn hạn
			return Ok(new { message = result, needsAuth = false });
		}
	}


		/// <summary>
		/// Callback khi Google redirect về, nhận code và lưu token vào DB chỉ Auth lấy refreshtoken để thực hiện create meeting FE không cần quan tâm
		/// </summary>

		[HttpGet("callback")]
		public async Task<IActionResult> Callback([FromQuery] string code)
		{
			if (string.IsNullOrEmpty(code))
				return BadRequest("Missing authorization code");

			var token = await _googleAuthService.ExchangeCodeForTokenDatAsync(code);

			return Ok(new
			{
				message = "Google account authorized successfully!",
				email = token.AccountEmail,
				hasRefreshToken = !string.IsNullOrEmpty(token.RefreshToken),
				expiresAt = token.ExpiresAt
			});
		}
		/// <summary>
		/// Test Tạo buổi học (Google Calendar event + Meet link)
		/// </summary>
		[HttpPost("create")]
		public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request)
		{
			if (request == null)
				return BadRequest("Invalid request data.");

			var result = await _googleCalendarService.CreateEventAsync(request);
			return Ok(result);
		}


	}
}
