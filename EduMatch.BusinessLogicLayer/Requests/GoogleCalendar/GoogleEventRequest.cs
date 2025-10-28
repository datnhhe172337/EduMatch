using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.GoogleCalendar
{
	public class GoogleEventRequest
	{
		public string OrganizerEmail { get; set; } = string.Empty;
		public string Summary { get; set; } = "Buổi học EduMatch";
		public string Description { get; set; } = "";
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public List<string>? Attendees { get; set; }
	}
}
