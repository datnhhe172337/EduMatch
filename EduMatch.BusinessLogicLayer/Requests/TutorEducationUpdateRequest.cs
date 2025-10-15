using EduMatch.DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorEducationUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Tutor ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

		[Required(ErrorMessage = "Institution ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Institution ID must be greater than 0")]
		public int InstitutionId { get; set; }

		public DateTime? IssueDate { get; set; }

		[StringLength(500, ErrorMessage = "Certificate URL cannot exceed 500 characters")]
		public string? CertificateUrl { get; set; }

		[StringLength(200, ErrorMessage = "Certificate public ID cannot exceed 200 characters")]
		public string? CertificatePublicId { get; set; }

		[Required(ErrorMessage = "Verification status is required")]
		public VerifyStatus Verified { get; set; }

		[StringLength(500, ErrorMessage = "Reject reason cannot exceed 500 characters")]
		public string? RejectReason { get; set; }
	}
}
