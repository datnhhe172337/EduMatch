using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class BookingDto
	{
		public int Id { get; set; }
		public string LearnerEmail { get; set; } = null!;
		public int TutorSubjectId { get; set; }
		public DateTime BookingDate { get; set; }
		public int TotalSessions { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal TotalAmount { get; set; }
		public PaymentStatus PaymentStatus { get; set; }
		public decimal RefundedAmount { get; set; }
		public BookingStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public SystemFeeDto SystemFee { get; set; }
		public decimal SystemFeeAmount { get; set; }
		public decimal TutorReceiveAmount { get; set; }

		// Optional nested objects
		public TutorSubjectDto? TutorSubject { get; set; }
		public List<ScheduleDto>? Schedules { get; set; }
	}
}
