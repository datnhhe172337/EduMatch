using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TutorsController : ControllerBase
	{

		private readonly ITutorSubjectService _tutorSubjectService;
		private readonly ISubjectService _subjectService;
		private readonly ILevelService _levelService;
		private readonly CurrentUserService _currentUserService;
		private readonly IEducationInstitutionService _educationInstitutionService;
		private readonly ITutorAvailabilityService _tutorAvailabilityService;
		private readonly ITutorProfileService _tutorProfileService;
		private readonly ITutorCertificateService _tutorCertificateService;

		public TutorsController(
			ITutorSubjectService tutorSubjectService,
			ISubjectService subjectService,
			ILevelService levelService,
			CurrentUserService currentUserService,
			IEducationInstitutionService educationInstitutionService,
			ITutorAvailabilityService tutorAvailabilityService,
			ITutorProfileService tutorProfileService,
			ITutorCertificateService tutorCertificateService

			)
		{
			_tutorSubjectService = tutorSubjectService;
			_subjectService = subjectService;
			_levelService = levelService;
			_currentUserService = currentUserService;
			_educationInstitutionService = educationInstitutionService;
			_tutorAvailabilityService = tutorAvailabilityService;
			_tutorProfileService = tutorProfileService;
			_tutorCertificateService = tutorCertificateService;

		}



		// became tutor
		[Authorize]
		[HttpPost("become-tutor")]

		public async Task<IActionResult> BecameTutor()
		{

			return Ok();
		}


	}
}
