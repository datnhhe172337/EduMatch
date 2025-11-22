using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
    public enum WithdrawalStatus
    {
        Pending = 0,      // Chờ duyệt
        Approved = 1,     // Đã duyệt (Đang xử lý chuyển tiền)
        Rejected = 2,     // Bị từ chối
        Completed = 3,    // Hoàn thành (Tiền đã chuyển thành công)
        Failed = 4        // Thất bại (Chuyển tiền không thành công)
    }
}

