using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest
{
	public class TutorVerificationRequestCreateRequest
	{
		[Required(ErrorMessage = "UserEmail là bắt buộc")]
		[EmailAddress(ErrorMessage = "UserEmail phải là email hợp lệ")]
		public string UserEmail { get; set; } = null!;

		[Range(1, int.MaxValue, ErrorMessage = "TutorId phải lớn hơn 0")]
		public int? TutorId { get; set; }

		[MaxLength(1000, ErrorMessage = "Description không được vượt quá 1000 ký tự")]
		public string? Description { get; set; }
	}
}

