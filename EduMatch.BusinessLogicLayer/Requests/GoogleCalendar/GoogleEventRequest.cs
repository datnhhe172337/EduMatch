using Newtonsoft.Json;
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

		[JsonProperty("summary")]
		public string Summary { get; set; } = "Buổi học EduMatch";

		[JsonProperty("description")]
		public string Description { get; set; } = string.Empty;

		[JsonProperty("start")]
		public GoogleEventDateTime Start { get; set; } = new();

		[JsonProperty("end")]
		public GoogleEventDateTime End { get; set; } = new();

		[JsonProperty("attendees")]
		public List<GoogleAttendee>? Attendees { get; set; }

		[JsonProperty("conferenceData")]
		public GoogleConferenceData? ConferenceData { get; set; }
	}

	public class GoogleEventDateTime
	{
		[JsonProperty("dateTime")]
		public string DateTime { get; set; } = string.Empty;

		[JsonProperty("timeZone")]
		public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
	}

	public class GoogleAttendee
	{
		[JsonProperty("email")]
		public string Email { get; set; } = string.Empty;
	}

	public class GoogleConferenceData
	{
		[JsonProperty("createRequest")]
		public GoogleConferenceCreateRequest CreateRequest { get; set; } = new();
	}

	public class GoogleConferenceCreateRequest
	{
		[JsonProperty("requestId")]
		public string RequestId { get; set; } = Guid.NewGuid().ToString();

		[JsonProperty("conferenceSolutionKey")]
		public ConferenceSolutionKey ConferenceSolutionKey { get; set; } = new();
	}

	public class ConferenceSolutionKey
	{
		[JsonProperty("type")]
		public string Type { get; set; } = "hangoutsMeet";
	}

}
