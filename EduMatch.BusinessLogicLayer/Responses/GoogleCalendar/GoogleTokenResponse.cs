using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses.GoogleCalendar
{
	public class GoogleTokenResponse
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; set; } = string.Empty;

		[JsonProperty("refresh_token")]
		public string? RefreshToken { get; set; }

		[JsonProperty("expires_in")]
		public int ExpiresIn { get; set; }

		[JsonProperty("scope")]
		public string? Scope { get; set; }

		[JsonProperty("token_type")]
		public string? TokenType { get; set; }
	}
}
