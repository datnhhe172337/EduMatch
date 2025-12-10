using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum ScheduleStatus
	{
		Upcoming = 0,    // Sắp diễn ra
		InProgress = 1,  // Đang học
		Pending = 2,     // Chờ xử lý
		Processing = 3,   // Đang xử lý
		Completed = 4,   // Hoàn thành
		Cancelled = 5,   // Đã hủy

	}
}

