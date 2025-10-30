using System;
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
        public DateTime? BookingDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Tổng số buổi học phải >= 1")]
        public int? TotalSessions { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "PaymentStatus phải là giá trị hợp lệ của PaymentStatus")]
        public PaymentStatus? PaymentStatus { get; set; }
        public decimal? RefundedAmount { get; set; }
        [EnumDataType(typeof(BookingStatus), ErrorMessage = "Status phải là giá trị hợp lệ của BookingStatus")]
        public BookingStatus? Status { get; set; }
    }
}
