using EduMatch.DataAccessLayer.Enum;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class BecomeTutorRequest
	{
		// ----- PROFILE -----
		[Required(ErrorMessage = "Tutor profile is required.")]
		public TutorProfileCreateRequest TutorProfile { get; set; } = new();

		// ----- EDUCATION -----
		
		 [Required(ErrorMessage = "At least one education entry is required.")]
		public List<TutorEducationCreateRequest>? Educations { get; set; } = new();

		// ----- CERTIFICATES -----
		public List<TutorCertificateCreateRequest>? Certificates { get; set; } = new();

		// ----- SUBJECTS -----
		public List<TutorSubjectCreateRequest>? Subjects { get; set; } = new();

		// ----- AVAILABILITY -----

		[Required(ErrorMessage = "Availability information is required.")]
		public List<TutorAvailabilityCreateRequest> Availabilities { get; set; } = new();
	}
}
