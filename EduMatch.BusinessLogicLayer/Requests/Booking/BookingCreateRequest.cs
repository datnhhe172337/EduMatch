using System;
using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.Booking
{
    public class BookingCreateRequest
    {
        [Required(ErrorMessage = "LearnerEmail là bắt buộc")]
        [EmailAddress(ErrorMessage = "LearnerEmail không đúng định dạng email")]
        public string LearnerEmail { get; set; } = null!;

        [Required(ErrorMessage = "TutorSubjectId là bắt buộc")]
        public int TutorSubjectId { get; set; }

        [Required(ErrorMessage = "BookingDate là bắt buộc")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "TotalSessions là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Tổng số buổi học phải >= 1")]
        public int TotalSessions { get; set; }

        [Required(ErrorMessage = "UnitPrice là bắt buộc")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "TotalAmount là bắt buộc")]
        public decimal TotalAmount { get; set; }

        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "PaymentStatus phải là giá trị hợp lệ của PaymentStatus")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [EnumDataType(typeof(BookingStatus), ErrorMessage = "Status phải là giá trị hợp lệ của BookingStatus")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
