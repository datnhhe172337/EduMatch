using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorCertificate
{
	public class TutorCertificateCreateRequest
	{
		
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

	
		[Range(1, int.MaxValue, ErrorMessage = "Certificate type ID must be greater than 0")]
		public int CertificateTypeId { get; set; }

		public DateTime? IssueDate { get; set; }

		public DateTime? ExpiryDate { get; set; }

		// Allow either file upload or remote URL. Only one is required.
		//public IFormFile? Certificate { get; set; }

		[Url(ErrorMessage = "Certificate URL must be a valid URL.")]
		public string? CertificateUrl { get; set; }



	}
}
