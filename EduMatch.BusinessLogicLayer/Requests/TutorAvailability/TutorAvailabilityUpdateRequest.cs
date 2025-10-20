using EduMatch.DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorAvailability
{
	public class TutorAvailabilityUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Tutor ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

		[Required(ErrorMessage = "Slot ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Slot ID must be greater than 0")]
		public int SlotId { get; set; }

		public TutorAvailabilityStatus? Status { get; set; }

		[Required(ErrorMessage = "Start date is required")]
		public DateTime StartDate { get; set; }

	}
}
