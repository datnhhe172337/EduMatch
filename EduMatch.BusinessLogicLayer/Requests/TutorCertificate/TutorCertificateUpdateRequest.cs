using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorCertificate
{
	public class TutorCertificateUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Tutor ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Certificate type ID must be greater than 0")]
		public int CertificateTypeId { get; set; }

		public DateTime? IssueDate { get; set; }

		public DateTime? ExpiryDate { get; set; }

		// Optional: either provide a new file or keep/update URL
		//public IFormFile? Certificate { get; set; }

		[Url(ErrorMessage = "Certificate URL must be a valid URL.")]
		public string? CertificateUrl { get; set; }


		[EnumDataType(typeof(VerifyStatus), ErrorMessage = "Invalid verify status")]
		public VerifyStatus? Verified { get; set; }

		[StringLength(500, ErrorMessage = "Reject reason cannot exceed 500 characters")]
		public string? RejectReason { get; set; }

		
	}
}
