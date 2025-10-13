using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorCertificateUpdateRequest
	{
		public int Id { get; set; }
		public int TutorId { get; set; }
		public int CertificateTypeId { get; set; }
		public DateTime? IssueDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public string? CertificateUrl { get; set; }
		public string? CertificatePublicId { get; set; }
		public VerifyStatus Verified { get; set; }
		public string? RejectReason { get; set; }
	}
}
