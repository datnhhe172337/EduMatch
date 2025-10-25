using Microsoft.AspNetCore.Http;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests
{
    // The main request object that wraps everything
    public class UpdateTutorProfileRequest
    {
        public UpdateCoreTutorProfileRequest? Profile { get; set; }
        public List<UpdateTutorEducationRequest>? Educations { get; set; }
        public List<UpdateTutorCertificateRequest>? Certificates { get; set; }
        public List<UpdateTutorSubjectRequest>? Subjects { get; set; }
        public List<UpdateTutorAvailabilityRequest>? Availabilities { get; set; }
    }

    // DTO for updating the core profile
    public class UpdateCoreTutorProfileRequest
    {
        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(30)]
        [RegularExpression(@"^(?:0\d{9}|(?:\+?84)\d{9,10})$")]
        public string? Phone { get; set; }

        [MaxLength(2000)]
        public string? Bio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public bool RemoveAvatar { get; set; } = false; // Flag to delete

        [Range(1, int.MaxValue)]
        public int? ProvinceId { get; set; }

        [Range(1, int.MaxValue)]
        public int? SubDistrictId { get; set; }

        [MaxLength(2000)]
        public string? TeachingExp { get; set; }

        [EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        [MaxLength(500)] // Add validation if needed
        public string? AddressLine { get; set; }
        public IFormFile? VideoIntro { get; set; }
        public bool RemoveVideoIntro { get; set; } = false; // Flag to delete

        [Url]
        public string? VideoIntroUrl { get; set; }

        [EnumDataType(typeof(TeachingMode))]
        public TeachingMode? TeachingModes { get; set; }
    }

    // DTO for updating Education
    public class UpdateTutorEducationRequest
    {
        [Required]
        public int Id { get; set; } // 0 = New, >0 = Existing

        [Required]
        [Range(1, int.MaxValue)]
        public int InstitutionId { get; set; }

        public DateTime? IssueDate { get; set; }
        public IFormFile? CertificateEducation { get; set; }
        public bool RemoveCertificate { get; set; } = false; // Flag to delete
    }

    // DTO for updating Certificate
    public class UpdateTutorCertificateRequest
    {
        [Required]
        public int Id { get; set; } // 0 = New, >0 = Existing

        [Required]
        [Range(1, int.MaxValue)]
        public int CertificateTypeId { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public IFormFile? Certificate { get; set; }
        public bool RemoveCertificate { get; set; } = false; // Flag to delete
    }

    // DTO for updating Subject
    public class UpdateTutorSubjectRequest
    {
        [Required]
        public int Id { get; set; } // 0 = New, >0 = Existing

        [Required]
        [Range(1, int.MaxValue)]
        public int SubjectId { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal HourlyRate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int LevelId { get; set; }
    }

    // DTO for updating Availability
    public class UpdateTutorAvailabilityRequest
    {
        [Required]
        public int Id { get; set; } // 0 = New, >0 = Existing

        [Required]
        [Range(1, int.MaxValue)]
        public int SlotId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        // Note: Add EndDate if your logic requires it
        // public DateTime EndDate { get; set; }
    }
}