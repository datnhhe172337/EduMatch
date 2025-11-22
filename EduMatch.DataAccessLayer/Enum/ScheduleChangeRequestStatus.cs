using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum ScheduleChangeRequestStatus
	{
		Pending = 0,      // Chờ xử lý
		Approved = 1,     // Đã chấp nhận
		Rejected = 2,     // Đã từ chối
		Cancelled = 3     // Đã hủy
	}
}

