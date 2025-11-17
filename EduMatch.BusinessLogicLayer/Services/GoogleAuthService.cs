using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _config;

		private readonly GoogleCalendarSettings _googleCalendarSettings;
		private readonly IGoogleTokenRepository _googleTokenRepository;
		private readonly HttpClient _httpClient;
		public GoogleAuthService(IConfiguration config, IOptions<GoogleCalendarSettings> googleCalendarSettings, IGoogleTokenRepository googleTokenRepository, HttpClient httpClient)
        {
            _config = config;
			_googleCalendarSettings = googleCalendarSettings.Value;
			_googleTokenRepository = googleTokenRepository;
			_httpClient = httpClient;
		}

		public async Task<GoogleToken> ExchangeCodeForTokenDatAsync(string code)
		{
			var body = new Dictionary<string, string>
				{
					{ "code", code },
					{ "client_id", _googleCalendarSettings.ClientId },
					{ "client_secret", _googleCalendarSettings.ClientSecret },
					{ "redirect_uri", _googleCalendarSettings.RedirectUri },
					{ "grant_type", "authorization_code" }
				};

			var response = await _httpClient.PostAsync(
				_googleCalendarSettings.TokenEndpoint,
				new FormUrlEncodedContent(body)
			);

			var json = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				throw new Exception($"Failed to exchange code: {json}");

			var tokenResp = JsonConvert.DeserializeObject<GoogleTokenResponse>(json)
				?? throw new Exception("Invalid token response");

			string email = _googleCalendarSettings.SystemAccountEmail;

			var existing = await _googleTokenRepository.GetByEmailAsync(email);

			if (existing == null)
			{
				var newToken = new GoogleToken
				{
					AccountEmail = email,
					AccessToken = tokenResp.AccessToken,
					RefreshToken = tokenResp.RefreshToken,
					TokenType = tokenResp.TokenType,
					Scope = tokenResp.Scope,
					ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResp.ExpiresIn),
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _googleTokenRepository.CreateAsync(newToken);
				return newToken;
			}
			else
			{
				existing.AccessToken = tokenResp.AccessToken;
				existing.TokenType = tokenResp.TokenType;
				existing.Scope = tokenResp.Scope;
				existing.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResp.ExpiresIn);
				existing.UpdatedAt = DateTime.UtcNow;

				// refresh_token chỉ trả về lần đầu — nếu có thì cập nhật
				if (!string.IsNullOrEmpty(tokenResp.RefreshToken))
					existing.RefreshToken = tokenResp.RefreshToken;

				await _googleTokenRepository.UpdateAsync(existing);
				return existing;
			}
		}
		

		//public string GenerateAuthUrlDat()
		//{
		//	string scopes = string.Join(" ",
		//		new[]
		//		{
		//			"https://www.googleapis.com/auth/calendar.events",
		//			"https://www.googleapis.com/auth/calendar"
		//		});

		//	string url =
		//		"https://accounts.google.com/o/oauth2/v2/auth" +
		//		"?response_type=code" +
		//		$"&client_id={_googleCalendarSettings.ClientId}" +
		//		$"&redirect_uri={Uri.EscapeDataString(_googleCalendarSettings.RedirectUri)}" +
		//		$"&scope={Uri.EscapeDataString(scopes)}" +
		//		"&access_type=offline" +
		//		"&prompt=consent" + // ép lấy refresh_token
		//		$"&login_hint={Uri.EscapeDataString(_googleCalendarSettings.SystemAccountEmail)}";

		//	return url;
		//}

	public async Task<string> GenerateAuthUrlDat()
	{
		string scopes = string.Join(" ",
			new[]
			{
		"https://www.googleapis.com/auth/calendar.events",
		"https://www.googleapis.com/auth/calendar"
			});

		var existingToken = await _googleTokenRepository.GetByEmailAsync(_googleCalendarSettings.SystemAccountEmail);

		// Kiểm tra nếu không có refresh token
		if (string.IsNullOrEmpty(existingToken?.RefreshToken))
		{
			// Không có refresh token, trả về URL để lấy token mới
			return BuildAuthUrl(scopes, true);
		}

		try
		{
			// Kiểm tra xem refresh token có bị revoked không
			bool isRevoked = await IsRefreshTokenRevokedAsync(existingToken);
			if (isRevoked)
			{
				// Token bị revoked, trả về URL để lấy token mới
				return BuildAuthUrl(scopes, true);
			}
			else
			{
				// Refresh token vẫn còn hạn, trả về message
				return "Refresh token vẫn còn hạn và hợp lệ. Không cần cấp quyền lại.";
			}
		}
		catch (Exception ex)
		{
			// Nếu có lỗi khi kiểm tra, log và trả về URL để an toàn
			Console.WriteLine($"[GoogleAuthService] Error validating refresh token in GenerateAuthUrlDat: {ex.Message}");
			Console.WriteLine($"[GoogleAuthService] Stack trace: {ex.StackTrace}");
			return BuildAuthUrl(scopes, true);
		}
	}

	/// <summary>
	/// Tạo auth URL cho Google OAuth
	/// </summary>
	private string BuildAuthUrl(string scopes, bool needsConsent)
	{
		string prompt = needsConsent
			? "&prompt=consent" // yêu cầu lại quyền khi chưa có refresh token hoặc token bị revoked
			: "";

		string url =
			"https://accounts.google.com/o/oauth2/v2/auth" +
			"?response_type=code" +
			$"&client_id={_googleCalendarSettings.ClientId}" +
			$"&redirect_uri={Uri.EscapeDataString(_googleCalendarSettings.RedirectUri)}" +
			$"&scope={Uri.EscapeDataString(scopes)}" +
			"&access_type=offline" +
			prompt +
			$"&login_hint={Uri.EscapeDataString(_googleCalendarSettings.SystemAccountEmail)}";

		return url;
	}

	/// <summary>
	/// Kiểm tra xem refresh token có bị revoked hoặc invalid không
	/// </summary>
	private async Task<bool> IsRefreshTokenRevokedAsync(GoogleToken token)
	{
		if (string.IsNullOrEmpty(token.RefreshToken))
			return true;

		try
		{
			var refreshRequest = new Dictionary<string, string>
			{
				{ "client_id", _googleCalendarSettings.ClientId },
				{ "client_secret", _googleCalendarSettings.ClientSecret },
				{ "refresh_token", token.RefreshToken },
				{ "grant_type", "refresh_token" }
			};

			var response = await _httpClient.PostAsync(
				_googleCalendarSettings.TokenEndpoint,
				new FormUrlEncodedContent(refreshRequest)
			);

			var json = await response.Content.ReadAsStringAsync();

			// Nếu response không thành công và chứa "invalid_grant", token đã bị revoked
			if (!response.IsSuccessStatusCode)
			{
				// Log lỗi đầy đủ để debug
				Console.WriteLine($"[GoogleAuthService] Failed to refresh token. Status: {response.StatusCode}, Response: {json}");

				if (json.Contains("invalid_grant") || json.Contains("Token has been expired or revoked"))
				{
					Console.WriteLine($"[GoogleAuthService] Refresh token đã bị revoked hoặc expired. Xóa token khỏi DB.");
					// Xóa refresh token vì đã không còn hợp lệ
					token.RefreshToken = null;
					token.AccessToken = null;
					token.ExpiresAt = null;
					token.UpdatedAt = DateTime.UtcNow;
					await _googleTokenRepository.UpdateAsync(token);
					return true;
				}
				else
				{
					// Có lỗi khác, throw exception để biết lỗi gì
					throw new Exception($"Failed to refresh Google access token. Status: {response.StatusCode}, Response: {json}");
				}
			}

			return false;
		}
		catch (Exception ex)
		{
			// Log lỗi đầy đủ để debug
			Console.WriteLine($"[GoogleAuthService] Error checking refresh token validity: {ex.Message}");
			Console.WriteLine($"[GoogleAuthService] Stack trace: {ex.StackTrace}");
			
			// Throw lại exception để caller biết lỗi gì
			throw new Exception($"Error validating refresh token: {ex.Message}", ex);
		}
	}





		public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _config["GoogleAuth:ClientId"] }
            };

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GoogleAuthService] Invalid Google token: {ex.Message}");
                return null;
            }
        }
    }
}
