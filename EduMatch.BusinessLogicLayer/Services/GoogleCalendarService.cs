using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Requests.GoogleMeeting;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
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

			// chuyển sang dictionary để gửi form-urlencoded
			var formContent = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("client_id", refreshRequest.ClientId),
				new KeyValuePair<string, string>("client_secret", refreshRequest.ClientSecret),
				new KeyValuePair<string, string>("refresh_token", refreshRequest.RefreshToken),
				new KeyValuePair<string, string>("grant_type", refreshRequest.GrantType),
			});

			var response = await _httpClient.PostAsync(_googleCalendarSettings.TokenEndpoint, formContent);
			var json = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				// Kiểm tra nếu token bị revoked hoặc expired
				if (json.Contains("invalid_grant") || json.Contains("Token has been expired or revoked"))
				{
					// Xóa refresh token và access token vì đã không còn hợp lệ
					token.RefreshToken = null;
					token.AccessToken = null;
					token.ExpiresAt = null;
					token.UpdatedAt = DateTime.UtcNow;
					await _googleTokenRepository.UpdateAsync(token);
					throw new Exception("Google refresh token đã bị thu hồi hoặc hết hạn. Vui lòng cấp quyền lại Google Calendar cho hệ thống.");
				}
				throw new Exception($"Failed to refresh Google access token: {json}");
			}

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
		public async Task<GoogleEventCreatedResponse?> CreateEventAsync(CreateMeetingRequest request)
		{
			var eventRequest = BuildGoogleEvent(request);

			string accessToken = await GetAccessTokenAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			string calendarId = _googleCalendarSettings.SystemAccountEmail;
			string endpoint = $"{_googleCalendarSettings.CalendarApiBaseUrl}/calendars/{calendarId}/events?conferenceDataVersion=1&sendUpdates=none";

			var jsonBody = JsonConvert.SerializeObject(eventRequest);
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
		public async Task<GoogleEventCreatedResponse?> UpdateEventAsync(string eventId, CreateMeetingRequest request)
		{

			var eventRequest = BuildGoogleEvent(request);

			string accessToken = await GetAccessTokenAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			string calendarId = _googleCalendarSettings.SystemAccountEmail;
			string endpoint = $"{_googleCalendarSettings.CalendarApiBaseUrl}/calendars/{calendarId}/events/{eventId}?conferenceDataVersion=1&sendUpdates=none";

			var jsonBody = JsonConvert.SerializeObject(eventRequest);
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

		/// <summary>
		/// Mappping create meeting request sang google event request
		/// </summary>
		private GoogleEventRequest BuildGoogleEvent(CreateMeetingRequest req)
		{
			return new GoogleEventRequest
			{
				OrganizerEmail = _googleCalendarSettings.SystemAccountEmail,
				Summary = req.Summary,
				Description = req.Description ?? string.Empty,
				Start = new GoogleEventDateTime
				{
					DateTime = req.StartTime.AddMinutes(-15).ToString("yyyy-MM-ddTHH:mm:ss"),
					TimeZone = "Asia/Ho_Chi_Minh"
				},
				End = new GoogleEventDateTime
				{
					DateTime = req.EndTime.AddMinutes(15).ToString("yyyy-MM-ddTHH:mm:ss"),
					TimeZone = "Asia/Ho_Chi_Minh"
				},
				Attendees = req.AttendeeEmails
					.Select(e => new GoogleAttendee { Email = e })
					.ToList(),
				ConferenceData = new GoogleConferenceData
				{
					CreateRequest = new GoogleConferenceCreateRequest
					{
						RequestId = Guid.NewGuid().ToString(),
						ConferenceSolutionKey = new ConferenceSolutionKey
						{
							Type = "hangoutsMeet"
						}
					}
				}
			};
		}

	}
}
