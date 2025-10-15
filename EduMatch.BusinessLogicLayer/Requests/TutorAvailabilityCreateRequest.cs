using EduMatch.DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorAvailabilityCreateRequest
	{
		[Required(ErrorMessage = "Tutor ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

		[Required(ErrorMessage = "Day of week is required")]
		public DayOfWeek DayOfWeek { get; set; }

		[Required(ErrorMessage = "Slot ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Slot ID must be greater than 0")]
		public int SlotId { get; set; }

		public bool IsRecurring { get; set; }

		[Required(ErrorMessage = "Effective from date is required")]
		public DateTime EffectiveFrom { get; set; }

		public DateTime? EffectiveTo { get; set; }

		[CustomValidation(typeof(TutorAvailabilityCreateRequest), "ValidateDateRange")]
		public bool IsValidDateRange => !EffectiveTo.HasValue || EffectiveTo.Value >= EffectiveFrom;
	}
}
