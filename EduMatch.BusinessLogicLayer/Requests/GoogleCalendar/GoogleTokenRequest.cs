	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	namespace EduMatch.BusinessLogicLayer.Requests.GoogleCalendar
	{
		public class GoogleTokenRequest
		{
			[JsonProperty("client_id")]
			public string ClientId { get; set; } = string.Empty;

			[JsonProperty("client_secret")]
			public string ClientSecret { get; set; } = string.Empty;

			[JsonProperty("refresh_token")]
			public string RefreshToken { get; set; } = string.Empty;

			[JsonProperty("grant_type")]
			public string GrantType { get; set; } = "refresh_token"; 
		}
	}
