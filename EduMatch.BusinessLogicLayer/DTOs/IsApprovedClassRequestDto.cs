using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class IsApprovedClassRequestDto
    {
        public bool IsApproved { get; set; }
        //public DateTime ApprovedAt { get; set; }
        //public string ApprovedBy { get; set; }

        public string? RejectionReason { get; set; } // null nếu duyệt
    }
}
