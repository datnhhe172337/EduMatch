using System;

namespace EduMatch.DataAccessLayer.Enum
{
    public enum ScheduleCompletionStatus : byte
    {
        PendingConfirm = 0,
        LearnerConfirmed = 1,
        AutoCompleted = 2,
        ReportedOnHold = 3,
        Cancelled = 4
    }
}
