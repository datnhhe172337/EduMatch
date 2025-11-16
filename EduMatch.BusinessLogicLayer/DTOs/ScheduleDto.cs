using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class ScheduleDto
	{
		public int Id { get; set; }
		public int AvailabilitiId { get; set; }
		public int BookingId { get; set; }
		public ScheduleStatus Status { get; set; }
		public string? AttendanceNote { get; set; }
		public bool IsRefunded { get; set; }
		public DateTime? RefundedAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		public TutorAvailabilityDto? Availability { get; set; }
		public MeetingSessionDto? MeetingSession { get; set; }
	}
}
