using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum TutorAvailabilityStatus
	{
		/// 0 - Lịch trống, có thể được đặt.
	
		Available = 0,

		/// 1 - Lịch đã được học viên đặt nhưng chưa diễn ra.
	
		Booked = 1,

		/// 2 - Đang có buổi học diễn ra hoặc đã được xác nhận học.
	
		InProgress = 2,

		/// 3 - Lịch đã bị hủy bởi tutor hoặc hệ thống.
		Cancelled = 3
	}
}

