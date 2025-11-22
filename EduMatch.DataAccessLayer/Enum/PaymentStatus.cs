using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	public enum PaymentStatus
	{
		Pending = 0,        // Chưa thanh toán
		Paid = 1,           // Đã thanh toán
		RefundPending = 2,  // Chờ hoàn tiền
		Refunded = 3        // Đã hoàn tiền
	}
}

