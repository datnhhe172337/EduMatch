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

        /// <summary>
        /// Đặt booking học thử (miễn phí) 1 buổi cho môn học của gia sư.
        /// Mỗi learner chỉ được 1 buổi học thử cho mỗi cặp (Tutor, Subject).
        /// Nếu IsTrial = true thì TotalSessions bắt buộc = 1 và Booking sẽ có TotalAmount = 0.
        /// </summary>
        public bool IsTrial { get; set; } = false;

    }
}
