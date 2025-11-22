using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class BookingRefundRequestDto
	{
		public int Id { get; set; }
		public int BookingId { get; set; }
		public string LearnerEmail { get; set; } = null!;
		public int RefundPolicyId { get; set; }
		public string? Reason { get; set; }
		public BookingRefundRequestStatus Status { get; set; }
		public decimal? ApprovedAmount { get; set; }
		public string? AdminNote { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? ProcessedBy { get; set; }

		// Optional nested objects
		public BookingDto? Booking { get; set; }
		public RefundPolicyDto? RefundPolicy { get; set; }
		public UserDto? Learner { get; set; }
		public List<RefundRequestEvidenceDto>? RefundRequestEvidences { get; set; }
	}
}

