using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
		public virtual string Email =>
				_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
				?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("email")
				?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
				?? string.Empty;

	}
}
