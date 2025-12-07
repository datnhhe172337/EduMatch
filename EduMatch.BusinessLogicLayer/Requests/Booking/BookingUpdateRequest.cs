using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.Booking
{
    public class BookingUpdateRequest
    {
        [Required(ErrorMessage = "Id là bắt buộc")]
        public int Id { get; set; }

        [EmailAddress(ErrorMessage = "LearnerEmail không đúng định dạng email")]
        public string? LearnerEmail { get; set; }

        public int? TutorSubjectId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Tổng số buổi học phải >= 1")]
        public int? TotalSessions { get; set; }
    }
}
