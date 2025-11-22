using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest
{
	public class TutorVerificationRequestUpdateRequest
	{
		[Required(ErrorMessage = "Id là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Id phải lớn hơn 0")]
		public int Id { get; set; }

		[MaxLength(1000, ErrorMessage = "Description không được vượt quá 1000 ký tự")]
		public string? Description { get; set; }

		[MaxLength(1000, ErrorMessage = "AdminNote không được vượt quá 1000 ký tự")]
		public string? AdminNote { get; set; }
	}
}

