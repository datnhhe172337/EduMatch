using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface IGoogleCalendarService
	{
		Task<string> RefreshAccessTokenAsync(string refreshToken);
		Task<GoogleEventCreatedResponse?> CreateEventAsync(GoogleEventRequest request);
		Task<GoogleEventCreatedResponse?> UpdateEventAsync(string eventId, GoogleEventRequest request);
		Task<bool> DeleteEventAsync(string eventId);
	}
}
