using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.RefundRequestEvidence
{
    public class RefundRequestEvidenceCreateRequest
    {
        [Required(ErrorMessage = "BookingRefundRequestId là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "BookingRefundRequestId phải lớn hơn 0")]
        public int BookingRefundRequestId { get; set; }

        [Required(ErrorMessage = "FileUrl là bắt buộc")]
        [Url(ErrorMessage = "FileUrl phải là URL hợp lệ")]
        public string FileUrl { get; set; } = null!;
    }
}

