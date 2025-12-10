using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class ScheduleCompletionDto
	{
		public int Id { get; set; }
		public int ScheduleId { get; set; }
		public int BookingId { get; set; }
		public int TutorId { get; set; }
		public string LearnerEmail { get; set; } = null!;
		public ScheduleCompletionStatus Status { get; set; }
		public DateTime ConfirmationDeadline { get; set; }
		public DateTime? ConfirmedAt { get; set; }
		public DateTime? AutoCompletedAt { get; set; }
		public int? ReportId { get; set; }
		public string? Note { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}

