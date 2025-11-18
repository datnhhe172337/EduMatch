using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.BookingRefundRequest
{
	public class BookingRefundRequestCreateRequest
	{
		[Required(ErrorMessage = "BookingId là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "BookingId phải lớn hơn 0")]
		public int BookingId { get; set; }

		[Required(ErrorMessage = "LearnerEmail là bắt buộc")]
		[EmailAddress(ErrorMessage = "LearnerEmail phải là email hợp lệ")]
		public string LearnerEmail { get; set; } = null!;

		[Required(ErrorMessage = "RefundPolicyId là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "RefundPolicyId phải lớn hơn 0")]
		public int RefundPolicyId { get; set; }

		[MaxLength(1000, ErrorMessage = "Reason không được vượt quá 1000 ký tự")]
		public string? Reason { get; set; }
	}
}

