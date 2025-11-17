using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum ScheduleChangeRequestStatus
	{
		/// <summary>
		/// Yêu cầu vừa được tạo, đang chờ bên kia duyệt
		/// </summary>
		Pending = 0,

		/// <summary>
		/// Đã được bên kia chấp nhận, buổi học sẽ được đổi lịch
		/// </summary>
		Approved = 1,

		/// <summary>
		/// Bên kia từ chối đổi lịch, giữ nguyên lịch cũ
		/// </summary>
		Rejected = 2,

		/// <summary>
		/// Người tạo yêu cầu hoặc hệ thống hủy yêu cầu này
		/// </summary>
		Cancelled = 3
	}
}
