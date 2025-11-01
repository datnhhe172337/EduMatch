using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
	public class GoogleCalendarSettings
	{
		public string ClientId { get; set; } = string.Empty;
		public string ClientSecret { get; set; } = string.Empty;
		public string RedirectUri { get; set; } = string.Empty;
		public string SystemAccountEmail { get; set; } = string.Empty;
		public string CalendarApiBaseUrl { get; set; } = "https://www.googleapis.com/calendar/v3";
		public string TokenEndpoint { get; set; } = "https://oauth2.googleapis.com/token";
	}
}
