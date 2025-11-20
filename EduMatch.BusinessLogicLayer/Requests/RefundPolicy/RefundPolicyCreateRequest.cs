using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.RefundPolicy
{
	public class RefundPolicyCreateRequest
	{
		[Required(ErrorMessage = "Name là bắt buộc")]
		[MaxLength(200, ErrorMessage = "Name không được vượt quá 200 ký tự")]
		public string Name { get; set; } = null!;

		[MaxLength(1000, ErrorMessage = "Description không được vượt quá 1000 ký tự")]
		public string? Description { get; set; }

		

		[Required(ErrorMessage = "RefundPercentage là bắt buộc")]
		[Range(0, 100, ErrorMessage = "RefundPercentage phải từ 0 đến 100")]
		public decimal RefundPercentage { get; set; }

	}
}

