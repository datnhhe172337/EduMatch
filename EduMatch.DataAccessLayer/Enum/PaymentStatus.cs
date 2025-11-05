using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum PaymentStatus
	{
		Pending = 0,    // Chưa thanh toán
		Paid = 1,       // Đã thanh toán
		Refunded = 2    // Hoàn tiền
	}
}
