using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
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

		// Optional: either provide a new file or keep/update URL
		public IFormFile? CertificateEducation{ get; set; }

	

		[Required(ErrorMessage = "Verification status is required")]
		public VerifyStatus Verified { get; set; }

		[StringLength(300, ErrorMessage = "Reject reason cannot exceed 300 characters")]
		public string? RejectReason { get; set; }
	}
}
