using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorAvailabilityMixedRequest
	{
		[Required]
		public int TutorId { get; set; }

		
		/// Danh sách ngày-slot không lặp lại
	
		public List<DaySlotRequest> NonRecurringDaySlots { get; set; } = new();

		
		/// Danh sách ngày-slot có lặp lại
		
		public List<RecurringScheduleRequest> RecurringSchedule { get; set; } = new();
	}

	
	/// Cho ngày cụ thể
	
	public class DaySlotRequest
	{
		[Required]
		public DateTime Date { get; set; }

		[Required]
		public List<int> SlotIds { get; set; } = new();
	}


	/// Cho lịch lặp theo thứ hàng tuần

	public class RecurringScheduleRequest
	{
		[Required]
		public DateTime StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		[Required]
		public List<RecurringDaySlots> DaySlots { get; set; } = new();
	}

	public class RecurringDaySlots
	{
		[Required]
		public DayOfWeek DayOfWeek { get; set; }

		[Required]
		public List<int> SlotIds { get; set; } = new();
	}


}
