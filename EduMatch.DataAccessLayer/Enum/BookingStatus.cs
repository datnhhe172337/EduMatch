using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum BookingStatus
	{
		Pending = 0,       // Chờ xác nhận
		Confirmed = 1,     // Đã xác nhận
		Completed = 2,     // Hoàn thành
		Cancelled = 3      // Đã hủy
	}
}

