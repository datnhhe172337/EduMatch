using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum VerifyStatus 
	{
		Pending = 0,   // Chờ duyệt
		Verified = 1,  // Đã xác minh
		Rejected = 2,  // Bị từ chối
		Expired = 3,   // Hết hạn
		Removed = 4    // Đã xóa / thu hồi
	}
}

