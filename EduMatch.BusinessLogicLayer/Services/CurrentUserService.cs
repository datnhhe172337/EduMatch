using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.AspNetCore.Http;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class CurrentUserService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CurrentUserService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		// Lấy UserId từ Claims
		public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(JwtClaimTypes.UserName)?.Value ?? string.Empty;

		// Lấy Email từ Claims
		public string Email => _httpContextAccessor.HttpContext?.User?.FindFirst(JwtClaimTypes.Email)?.Value ?? string.Empty;



	}
}
