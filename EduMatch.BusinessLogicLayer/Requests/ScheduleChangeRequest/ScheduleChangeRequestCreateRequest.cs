using EduMatch.DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest
{
	public class ScheduleChangeRequestCreateRequest
	{
		[Required(ErrorMessage = "ScheduleId là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "ScheduleId phải lớn hơn 0")]
		public int ScheduleId { get; set; }

		[Required(ErrorMessage = "RequesterEmail là bắt buộc")]
		[EmailAddress(ErrorMessage = "RequesterEmail phải là email hợp lệ")]
		public string RequesterEmail { get; set; } = null!;

		[Required(ErrorMessage = "RequestedToEmail là bắt buộc")]
		[EmailAddress(ErrorMessage = "RequestedToEmail phải là email hợp lệ")]
		public string RequestedToEmail { get; set; } = null!;

		[Required(ErrorMessage = "OldAvailabilitiId là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "OldAvailabilitiId phải lớn hơn 0")]
		public int OldAvailabilitiId { get; set; }

		[Required(ErrorMessage = "NewAvailabilitiId là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "NewAvailabilitiId phải lớn hơn 0")]
		public int NewAvailabilitiId { get; set; }

		[MaxLength(500, ErrorMessage = "Reason không được vượt quá 500 ký tự")]
		public string? Reason { get; set; }
	}
}

