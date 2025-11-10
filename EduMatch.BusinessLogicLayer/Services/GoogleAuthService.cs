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

			string prompt = string.IsNullOrEmpty(existingToken?.RefreshToken)
				? "&prompt=consent" // chỉ yêu cầu lại quyền khi chưa có refresh token
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
