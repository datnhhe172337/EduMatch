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
		public List<int>? TutorAvailabilitiesId { get; set; }
		public List<int>? TutorCertificatesId { get; set; }
		public List<int>? TutorEducationsId { get; set; }
		public List<int>? TutorSubjectId { get; set; }
	}
}
