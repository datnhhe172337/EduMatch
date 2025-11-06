using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TimeSlot
{
	public class TimeSlotCreateRequest
	{
		[Required(ErrorMessage = "Start time is required")]
		public TimeOnly StartTime { get; set; }

		[Required(ErrorMessage = "End time is required")]
		public TimeOnly EndTime { get; set; }

		[CustomValidation(typeof(TimeSlotCreateRequest), "ValidateTimeRange")]
		public bool IsValidTimeRange => EndTime > StartTime;
	}
}
