using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorProfileDto
	{
		public int Id { get; set; }
		public string UserEmail { get; set; } = null!;
		public string? Bio { get; set; }
		public string? AvatarUrl { get; set; }

		public DateTime? Dob { get; set	; }

		public Gender? Gender { get; set; }

		public string? AddressLine { get; set; }

		public ProvinceDto? Province { get; set; }

		public SubDistrictDto? SubDistrict { get; set; }

		public decimal? Latitude { get; set; }

		public decimal? Longitude { get; set; }

		public string? Phone { get; set; }
		public string? UserName { get; set; }
		public string? TeachingExp { get; set; }
		public string? VideoIntroUrl { get; set; }

		[JsonIgnore]
		public string? VideoIntroPublicId { get; set; }
		public TeachingMode TeachingModes { get; set; }
        public TutorStatus Status { get; set; }

		public string? VerifiedBy { get; set; }

		public DateTime? VerifiedAt { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public ICollection<TutorAvailabilityDto>? TutorAvailabilities { get; set; }
		public ICollection<TutorCertificateDto>? TutorCertificates { get; set; }
		public ICollection<TutorEducationDto>? TutorEducations { get; set; }
		public ICollection<TutorSubjectDto>? TutorSubjects { get; set; }
	}
}
