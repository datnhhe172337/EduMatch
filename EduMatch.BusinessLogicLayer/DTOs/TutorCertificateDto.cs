using EduMatch.DataAccessLayer.Enum;
using System;
using System.Text.Json.Serialization;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorCertificateDto
	{
		public int Id { get; set; }
		[JsonIgnore]
		public int TutorId { get; set; }
		[JsonIgnore]
		public int CertificateTypeId { get; set; }
		public DateTime? IssueDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public string? CertificateUrl { get; set; }
		[JsonIgnore]
		public string? CertificatePublicId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public VerifyStatus Verified { get; set; }
		public string? RejectReason { get; set; }
		public CertificateTypeDto? CertificateType { get; set; }
	}
}
