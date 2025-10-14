using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class BecomeTutorRequest
	{
		// ----- PROFILE -----
		public TutorProfileCreateRequest TutorProfile { get; set; } = new();

		// ----- EDUCATION -----
		public List<TutorEducationCreateRequest> Educations { get; set; } = new();

		// ----- CERTIFICATES -----
		public List<TutorCertificateCreateRequest> Certificates { get; set; } = new();

		// ----- SUBJECTS -----
		public List<TutorSubjectCreateRequest> Subjects { get; set; } = new();

		// ----- AVAILABILITY -----

		[Required(ErrorMessage = "Availability information is required.")]
		public TutorAvailabilityMixedRequest Availabilities { get; set; } = new();
	}
}
