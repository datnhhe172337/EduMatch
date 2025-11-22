using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
    public enum WalletTransactionReason
    {
        Deposit = 0,         // NẠP TIỀN
        Withdrawal = 1,      // RÚT TIỀN
        BookingPayment = 2,  // THANH TOÁN BOOKING
        BookingRefund = 3,   // HOÀN TIỀN BOOKING
        BookingPayout = 4,    // NHẬN TIỀN TỪ BOOKING (Tutor receives earnings)
        PlatformFee = 5
    }
}

