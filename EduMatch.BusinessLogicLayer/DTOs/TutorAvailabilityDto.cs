using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorAvailabilityDto
	{
		public int Id { get; set; }
		public int TutorId { get; set; }
		public DayOfWeek DayOfWeek { get; set; }
		public int SlotId { get; set; }
		public bool IsRecurring { get; set; }
		public DateTime EffectiveFrom { get; set; }
		public DateTime? EffectiveTo { get; set; }
		public TimeSlotDto? Slot { get; set; }
	}
}
