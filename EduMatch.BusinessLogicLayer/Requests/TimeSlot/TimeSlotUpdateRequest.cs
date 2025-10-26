using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TimeSlot
{
	public class TimeSlotUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Start time is required")]
		public TimeOnly StartTime { get; set; }

		[Required(ErrorMessage = "End time is required")]
		public TimeOnly EndTime { get; set; }

		[CustomValidation(typeof(TimeSlotUpdateRequest), "ValidateTimeRange")]
		public bool IsValidTimeRange => EndTime > StartTime;
	}
}
