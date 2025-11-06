using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Booking
{
    public class BookingCreateRequest
    {
        [Required(ErrorMessage = "LearnerEmail là bắt buộc")]
        [EmailAddress(ErrorMessage = "LearnerEmail không đúng định dạng email")]
        public string LearnerEmail { get; set; } = null!;

        [Required(ErrorMessage = "TutorSubjectId là bắt buộc")]
        public int TutorSubjectId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Tổng số buổi học phải >= 1")]
        public int? TotalSessions { get; set; }
        
    }
}
