using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.BookingRefundRequest
{
	public class BookingRefundRequestUpdateRequest
	{
		[Required(ErrorMessage = "Id là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Id phải lớn hơn 0")]
		public int Id { get; set; }

		[MaxLength(1000, ErrorMessage = "Reason không được vượt quá 1000 ký tự")]
		public string? Reason { get; set; }

		[MaxLength(1000, ErrorMessage = "AdminNote không được vượt quá 1000 ký tự")]
		public string? AdminNote { get; set; }
	}
}

