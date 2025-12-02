using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class RefundRequestEvidence
{
    public int Id { get; set; }

    public int BookingRefundRequestId { get; set; }

    public string FileUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual BookingRefundRequest BookingRefundRequest { get; set; } = null!;
}
