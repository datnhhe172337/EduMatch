using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.RefundPolicy
{
	public class RefundPolicyUpdateRequest
	{
		[Required(ErrorMessage = "Id là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Id phải lớn hơn 0")]
		public int Id { get; set; }

		[MaxLength(200, ErrorMessage = "Name không được vượt quá 200 ký tự")]
		public string? Name { get; set; }

		[MaxLength(1000, ErrorMessage = "Description không được vượt quá 1000 ký tự")]
		public string? Description { get; set; }

		[Range(0, 100, ErrorMessage = "RefundPercentage phải từ 0 đến 100")]
		public decimal? RefundPercentage { get; set; }


	}
}

