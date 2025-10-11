using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum CertificateType : byte
	{
		Unknown = 0, // Không xác định
		Teaching = 1, // Chứng chỉ giảng dạy
		Language = 2, // Ngoại ngữ
		Skill = 3, // Kỹ năng mềm
		Degree = 4, // Bằng cấp học thuật
		Other = 5  // Loại khác
	}
}
