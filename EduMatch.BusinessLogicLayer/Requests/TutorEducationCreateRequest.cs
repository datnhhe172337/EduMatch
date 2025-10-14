using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorEducationCreateRequest
	{
		[Required(ErrorMessage = "Tutor ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

		[Required(ErrorMessage = "Institution ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Institution ID must be greater than 0")]
		public int InstitutionId { get; set; }

		public DateTime? IssueDate { get; set; }

		[Required(ErrorMessage = "Certificate file is required")]
		public IFormFile CertificateEducation { get; set; }

	}
}
