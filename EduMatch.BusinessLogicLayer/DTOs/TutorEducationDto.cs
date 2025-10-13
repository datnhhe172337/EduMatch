using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorEducationDto
	{
		public int Id { get; set; }
		public int TutorId { get; set; }
		public int InstitutionId { get; set; }
		public DateTime? IssueDate { get; set; }
		public string? CertificateUrl { get; set; }
		public string? CertificatePublicId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public VerifyStatus Verified { get; set; }
		public string? RejectReason { get; set; }
		public EducationInstitutionDto? Institution { get; set; }
		public TutorProfileDto? Tutor { get; set; }
	}
}
