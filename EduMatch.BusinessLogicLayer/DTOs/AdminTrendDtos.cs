using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class SignupTrendPointDto
    {
        public DateOnly BucketDate { get; set; }
        public int Total { get; set; }
        public int Learners { get; set; }
        public int Tutors { get; set; }
    }

    public class BookingTrendPointDto
    {
        public DateOnly BucketDate { get; set; }
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
