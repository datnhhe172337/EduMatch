using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using EduMatch.DataAccessLayer.Entities;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface IGoogleAuthService
	{
		/// <summary>
		/// Xác thực idToken Google và trả về payload
		/// </summary>
		Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken);
		/// <summary>
		/// Tạo URL đăng nhập Google OAuth
		/// </summary>
		Task<string> GenerateAuthUrlDat();
		/// <summary>
		/// Đổi authorization code lấy GoogleToken
		/// </summary>
		Task<GoogleToken> ExchangeCodeForTokenDatAsync(string code);
	}
}
