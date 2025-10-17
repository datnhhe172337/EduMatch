using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorAvailabilityDto
	{
		public int Id { get; set; }
		public int TutorId { get; set; }
		public int SlotId { get; set; }
		public TutorAvailabilityStatus? Status { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public TimeSlotDto? Slot { get; set; }
		public TutorProfileDto? Tutor { get; set; }
	}
}
