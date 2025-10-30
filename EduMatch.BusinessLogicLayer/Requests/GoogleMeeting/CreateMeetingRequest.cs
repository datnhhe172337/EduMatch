using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.GoogleMeeting
{
	public class CreateMeetingRequest
	{
		public string Summary { get; set; } = "Buổi học EduMatch";
		public string? Description { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public List<string> AttendeeEmails { get; set; } = new();

	}

}
