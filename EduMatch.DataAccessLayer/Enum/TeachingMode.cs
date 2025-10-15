using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum TeachingMode : byte
	{
		Offline = 0, // Dạy trực tiếp
		Online = 1,  // Dạy online
		Hybrid = 2,  // Kết hợp
	}
}
