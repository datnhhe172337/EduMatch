using EduMatch.DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest
{
	public class ScheduleChangeRequestUpdateRequest
	{
		[Required(ErrorMessage = "Id là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Id phải lớn hơn 0")]
		public int Id { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "ScheduleId phải lớn hơn 0")]
		public int? ScheduleId { get; set; }

		[EmailAddress(ErrorMessage = "RequesterEmail phải là email hợp lệ")]
		public string? RequesterEmail { get; set; }

		[EmailAddress(ErrorMessage = "RequestedToEmail phải là email hợp lệ")]
		public string? RequestedToEmail { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "OldAvailabilitiId phải lớn hơn 0")]
		public int? OldAvailabilitiId { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "NewAvailabilitiId phải lớn hơn 0")]
		public int? NewAvailabilitiId { get; set; }

		[MaxLength(500, ErrorMessage = "Reason không được vượt quá 500 ký tự")]
		public string? Reason { get; set; }

	}
}

