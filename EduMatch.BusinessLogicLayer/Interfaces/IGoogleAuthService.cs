using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface IGoogleCalendarService
	{
		Task<GoogleEventResponse> CreateMeetingAsync(GoogleEventRequest req);
		Task<GoogleEventResponse> UpdateMeetingAsync(string eventId, GoogleEventRequest req);
		Task<bool> DeleteMeetingAsync(string eventId);
		Task<string> GetValidAccessTokenAsync(string accountEmail);
	}
}
