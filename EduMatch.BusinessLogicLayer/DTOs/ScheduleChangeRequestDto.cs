using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class ScheduleChangeRequestDto
	{
		public int Id { get; set; }
		public int ScheduleId { get; set; }
		public string RequesterEmail { get; set; } = null!;
		public string RequestedToEmail { get; set; } = null!;
		public int OldAvailabilitiId { get; set; }
		public int NewAvailabilitiId { get; set; }
		public string? Reason { get; set; }
		public ScheduleChangeRequestStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ProcessedAt { get; set; }

		// Optional nested objects
		public ScheduleDto? Schedule { get; set; }
		public TutorAvailabilityDto? OldAvailability { get; set; }
		public TutorAvailabilityDto? NewAvailability { get; set; }
	}
}

