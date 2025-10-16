using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorProfileDto
	{
		public int Id { get; set; }
		public string UserEmail { get; set; } = null!;
		public string? Bio { get; set; }
		public string? TeachingExp { get; set; }
		public string? VideoIntroUrl { get; set; }
		public string? VideoIntroPublicId { get; set; }
		public TeachingMode TeachingModes { get; set; }
		public TutorStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public ICollection<TutorAvailabilityDto>? TutorAvailabilities { get; set; }
		public ICollection<TutorCertificateDto>? TutorCertificates { get; set; }
		public ICollection<TutorEducationDto>? TutorEducations { get; set; }
		public ICollection<TutorSubjectDto>? TutorSubjects { get; set; }
	}
}
