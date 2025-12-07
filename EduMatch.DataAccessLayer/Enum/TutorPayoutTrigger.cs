using System;

namespace EduMatch.DataAccessLayer.Enum
{
    public enum TutorPayoutTrigger : byte
    {
        None = 0,
        LearnerConfirmed = 1,
        AutoCompleted = 2,
        AdminApproved = 3
    }
}
