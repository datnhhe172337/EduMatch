using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class GoogleCalendarService : IGoogleCalendarService
	{
		private readonly HttpClient _httpClient;
		private readonly IGoogleTokenRepository _googleTokenRepository;
		private readonly GoogleCalendarSettings _googleCalendarSettings;

		public GoogleCalendarService(
			HttpClient httpClient,
			IGoogleTokenRepository googleTokenRepository,
			IOptions<GoogleCalendarSettings> googleCalendarSettings)
		{
			_httpClient = httpClient;
			_googleTokenRepository = googleTokenRepository;
			_googleCalendarSettings = googleCalendarSettings.Value;
		}

		/// <summary>
		/// Lấy access token còn hạn từ DB hoặc tự refresh nếu hết hạn
		/// </summary>
		private async Task<string> GetAccessTokenAsync()
		{
			string systemEmail = _googleCalendarSettings.SystemAccountEmail;
			var token = await _googleTokenRepository.GetByEmailAsync(systemEmail)
				?? throw new Exception($"No Google token found for {systemEmail}");

			if (token.ExpiresAt.HasValue && token.ExpiresAt > DateTime.UtcNow.AddMinutes(1))
				return token.AccessToken ?? throw new Exception("Access token missing in database.");

			if (string.IsNullOrEmpty(token.RefreshToken))
				throw new Exception("Missing refresh token. Please re-authorize Google account.");

			return await RefreshAccessTokenAsync(token);
		}

		/// <summary>
		/// Làm mới access token bằng refresh token, cập nhật lại DB
		/// </summary>
		private async Task<string> RefreshAccessTokenAsync(GoogleToken token)
		{
			var refreshRequest = new GoogleTokenRequest
			{
				ClientId = _googleCalendarSettings.ClientId,
				ClientSecret = _googleCalendarSettings.ClientSecret,
				RefreshToken = token.RefreshToken!,
				GrantType = "refresh_token"
			};

			var content = new StringContent(JsonConvert.SerializeObject(refreshRequest), Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync(_googleCalendarSettings.TokenEndpoint, content);
			var json = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				throw new Exception($"Failed to refresh Google access token: {json}");

			var tokenResp = JsonConvert.DeserializeObject<GoogleTokenResponse>(json)
				?? throw new Exception("Invalid token response");

			token.AccessToken = tokenResp.AccessToken;
			token.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResp.ExpiresIn);
			token.Scope = tokenResp.Scope;
			token.TokenType = tokenResp.TokenType;
			token.UpdatedAt = DateTime.UtcNow;

			await _googleTokenRepository.UpdateAsync(token);

			return token.AccessToken!;
		}

		/// <summary>
		/// Tạo mới sự kiện (buổi học meeting) trong Calendar
		/// </summary>
		public async Task<GoogleEventCreatedResponse?> CreateEventAsync(GoogleEventRequest request)
		{
			string accessToken = await GetAccessTokenAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			string calendarId = _googleCalendarSettings.SystemAccountEmail;
			string endpoint = $"{_googleCalendarSettings.CalendarApiBaseUrl}/calendars/{calendarId}/events?conferenceDataVersion=1";

			var jsonBody = JsonConvert.SerializeObject(request);
			var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(endpoint, content);
			var responseJson = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				throw new Exception($"CreateEventAsync failed: {responseJson}");

			return JsonConvert.DeserializeObject<GoogleEventCreatedResponse>(responseJson);
		}

		/// <summary>
		/// Cập nhật sự kiện hiện có (ví dụ thay đổi giờ học, người tham gia, mô tả,...)
		/// </summary>
		public async Task<GoogleEventCreatedResponse?> UpdateEventAsync(string eventId, GoogleEventRequest request)
		{
			string accessToken = await GetAccessTokenAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			string calendarId = _googleCalendarSettings.SystemAccountEmail;
			string endpoint = $"{_googleCalendarSettings.CalendarApiBaseUrl}/calendars/{calendarId}/events/{eventId}?conferenceDataVersion=1";

			var jsonBody = JsonConvert.SerializeObject(request);
			var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

			var response = await _httpClient.PutAsync(endpoint, content);
			var responseJson = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				throw new Exception($"UpdateEventAsync failed: {responseJson}");

			return JsonConvert.DeserializeObject<GoogleEventCreatedResponse>(responseJson);
		}

		/// <summary>
		/// Xóa sự kiện khỏi Google Calendar
		/// </summary>
		public async Task<bool> DeleteEventAsync(string eventId)
		{
			string accessToken = await GetAccessTokenAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			string calendarId = _googleCalendarSettings.SystemAccountEmail;
			string endpoint = $"{_googleCalendarSettings.CalendarApiBaseUrl}/calendars/{calendarId}/events/{eventId}";

			var response = await _httpClient.DeleteAsync(endpoint);
			return response.IsSuccessStatusCode;
		}
	}
}
