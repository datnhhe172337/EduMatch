using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
    public enum ClassRequestStatus
    {
        Pending = 0,           // Đang chờ phê duyệt yêu cầu
        Open = 1,              // Đang mở (Đã được BA phê duyệt)
        Booked = 2,            // Đã booking thành công      
        Cancelled = 3,         // Hủy yêu cầu tạo lớp
        Expired  = 4,          // Đã quá hạn
        Rejected = 5           // BA từ chối yêu cầu
    }
}

