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
		Completed = 2,   // Hoàn thành
		Cancelled = 3,   // Đã hủy
		Absent = 4       // Vắng mặt
	}
}
