using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class RefundRequestEvidenceDto
    {
        public int Id { get; set; }
        public int BookingRefundRequestId { get; set; }
        public string FileUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Optional nested object
        public BookingRefundRequestDto? BookingRefundRequest { get; set; }
    }
}

