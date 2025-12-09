
using EduMatch.DataAccessLayer.Enum;

using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{

	public class TutorPayoutDto
	{
		public int Id { get; set; }
		public int ScheduleId { get; set; }
		public int BookingId { get; set; }
		public int TutorWalletId { get; set; }
		public decimal Amount { get; set; }
		public decimal SystemFeeAmount { get; set; }
		public TutorPayoutStatus Status { get; set; }
		public byte PayoutTrigger { get; set; }
		public DateOnly ScheduledPayoutDate { get; set; }
		public DateTime? ReleasedAt { get; set; }
		public int? WalletTransactionId { get; set; }
		public string? HoldReason { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}

