﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum TutorStatus 
	{
		Pending = 0,  // Chờ duyệt
		Approved = 1,  // Đã duyệt
		Rejected = 2,  // Bị từ chối
		Suspended = 3,  // Tạm khóa
		Deactivated = 4   // Ngừng hoạt động
	}
}
