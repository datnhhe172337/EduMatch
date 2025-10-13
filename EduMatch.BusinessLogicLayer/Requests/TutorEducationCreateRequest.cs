using System;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorEducationCreateRequest
	{
		public int TutorId { get; set; }
		public int InstitutionId { get; set; }
		public DateTime? IssueDate { get; set; }
		public string? CertificateUrl { get; set; }
		public string? CertificatePublicId { get; set; }
	}
}
