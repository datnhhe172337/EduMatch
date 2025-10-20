using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.TutorAvailability
{
	public class TutorAvailabilityMixedRequest
	{
		[Required]
		public int TutorId { get; set; }

		
		/// Danh sách ngày-slot không lặp lại
	
		public List<DaySlotRequest>? NonRecurringDaySlots { get; set; } = new();

		
		/// Danh sách ngày-slot có lặp lại
		
		public List<RecurringScheduleRequest>? RecurringSchedule { get; set; } = new();
	}

	
	/// Cho ngày cụ thể
	
	public class DaySlotRequest
	{
		
		public DateTime Date { get; set; }

		
		public List<int> SlotIds { get; set; } = new();
	}


	/// Cho lịch lặp theo thứ hàng tuần

	public class RecurringScheduleRequest
	{
		
		public DateTime StartDate { get; set; }

		public DateTime? EndDate { get; set; }

	
		public List<RecurringDaySlots> DaySlots { get; set; } = new();
	}

	public class RecurringDaySlots
	{
		
		public DayOfWeek DayOfWeek { get; set; }

		
		public List<int> SlotIds { get; set; } = new();
	}


}
