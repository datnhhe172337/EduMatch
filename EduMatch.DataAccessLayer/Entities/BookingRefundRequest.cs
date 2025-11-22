using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class BookingRefundRequest
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string LearnerEmail { get; set; } = null!;

    public int RefundPolicyId { get; set; }

    public string? Reason { get; set; }

    public int Status { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? ProcessedBy { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User LearnerEmailNavigation { get; set; } = null!;

    public virtual RefundPolicy RefundPolicy { get; set; } = null!;

    public virtual ICollection<RefundRequestEvidence> RefundRequestEvidences { get; set; } = new List<RefundRequestEvidence>();
}
