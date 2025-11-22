using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ClassRequest
{
    public int Id { get; set; }

    public string LearnerEmail { get; set; } = null!;

    public int SubjectId { get; set; }

    public string? Title { get; set; }

    public int LevelId { get; set; }

    public string? LearningGoal { get; set; }

    public string? TutorRequirement { get; set; }

    public TeachingMode Mode { get; set; }

    public int? ProvinceId { get; set; }

    public int? SubDistrictId { get; set; }

    public string? AddressLine { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateOnly? ExpectedStartDate { get; set; }

    public int ExpectedSessions { get; set; }

    public decimal? TargetUnitPriceMin { get; set; }

    public decimal? TargetUnitPriceMax { get; set; }

    public ClassRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public string? RejectionReason { get; set; }

    public string? CancelReason { get; set; }

    public virtual ICollection<ClassRequestSlotsAvailability> ClassRequestSlotsAvailabilities { get; set; } = new List<ClassRequestSlotsAvailability>();

    public virtual User LearnerEmailNavigation { get; set; } = null!;

    public virtual Level Level { get; set; } = null!;

    public virtual Province? Province { get; set; }

    public virtual SubDistrict? SubDistrict { get; set; }

    public virtual Subject Subject { get; set; } = null!;

    public virtual ICollection<TutorApplication> TutorApplications { get; set; } = new List<TutorApplication>();
}
