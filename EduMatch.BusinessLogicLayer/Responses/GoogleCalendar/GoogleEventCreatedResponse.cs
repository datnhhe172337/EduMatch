using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses.GoogleCalendar
{
	public class GoogleEventCreatedResponse
	{
		[JsonProperty("id")]
		public string EventId { get; set; } = string.Empty;

		[JsonProperty("htmlLink")]
		public string HtmlLink { get; set; } = string.Empty;

		[JsonProperty("hangoutLink")]
		public string? HangoutLink { get; set; }

		[JsonProperty("conferenceData")]
		public ConferenceData? ConferenceData { get; set; }
	}

	public class ConferenceData
	{
		[JsonProperty("conferenceId")]
		public string? ConferenceId { get; set; }

		[JsonProperty("entryPoints")]
		public List<EntryPoint>? EntryPoints { get; set; }
	}

	public class EntryPoint
	{
		[JsonProperty("entryPointType")]
		public string? EntryPointType { get; set; }

		[JsonProperty("uri")]
		public string? Uri { get; set; }

		[JsonProperty("label")]
		public string? Label { get; set; }
	}
}
