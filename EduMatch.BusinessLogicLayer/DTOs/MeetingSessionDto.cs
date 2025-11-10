using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class MeetingSessionDto
	{
		public int Id { get; set; }
		public int ScheduleId { get; set; }
		[JsonIgnore]
		public string OrganizerEmail { get; set; } = null!;
		public string MeetLink { get; set; } = null!;
		public string? MeetCode { get; set; }
		[JsonIgnore]
		public string? EventId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public MeetingType MeetingType { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
