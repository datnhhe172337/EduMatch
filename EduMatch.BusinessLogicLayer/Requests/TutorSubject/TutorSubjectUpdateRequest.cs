using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorSubject
{
	public class TutorSubjectUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Tutor ID is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Tutor ID must be greater than 0")]
		public int TutorId { get; set; }

		
		[Range(1, int.MaxValue, ErrorMessage = "Subject ID must be greater than 0")]
		public int SubjectId { get; set; }

		[Range(0, 999999.99, ErrorMessage = "Hourly rate must be between 0 and 999999.99")]
		public decimal? HourlyRate { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Level ID must be greater than 0")]
		public int? LevelId { get; set; }
	}
}
