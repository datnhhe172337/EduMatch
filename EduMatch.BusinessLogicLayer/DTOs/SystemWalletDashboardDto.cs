using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class SystemWalletDashboardDto
    {
        public decimal PendingTutorPayoutBalance { get; set; }

        public decimal PlatformRevenueBalance { get; set; }

        public decimal TotalUserAvailableBalance { get; set; }
    }
}
