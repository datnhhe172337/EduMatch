using System;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ReportEvidenceDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string? SubmittedByEmail { get; set; }
        public MediaType MediaType { get; set; }
        public ReportEvidenceType EvidenceType { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? FilePublicId { get; set; }
        public string? Caption { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
