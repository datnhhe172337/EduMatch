using System;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorCertificateCreateRequest
	{
		public int TutorId { get; set; }
		public int CertificateTypeId { get; set; }
		public DateTime? IssueDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public string? CertificateUrl { get; set; }
		public string? CertificatePublicId { get; set; }
	}
}
