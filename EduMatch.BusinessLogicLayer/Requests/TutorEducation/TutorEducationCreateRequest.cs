using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorEducation
{
	public class TutorEducationCreateRequest
	{
		
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

		
		[Range(1, int.MaxValue, ErrorMessage = "Institution ID must be greater than 0")]
		public int InstitutionId { get; set; }

		public DateTime? IssueDate { get; set; }

		// Allow either file upload or remote URL. Only one is required.
		//public IFormFile? CertificateEducation { get; set; }

		[Url(ErrorMessage = "Certificate URL must be a valid URL.")]
		public string? CertificateEducationUrl { get; set; }


	}
}
