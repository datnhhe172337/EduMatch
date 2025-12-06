using System;

namespace EduMatch.DataAccessLayer.Enum
{
    public enum TutorPayoutStatus : byte
    {
        Pending = 0,
        OnHold = 1,
        ReadyForPayout = 2,
        Paid = 3,
        Cancelled = 4
    }
}
