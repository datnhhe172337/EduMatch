using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class UpdateTutorProfileDto
    {
        // UserProfile properties
        public Gender? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string? AddressLine { get; set; }
        public int? SubDistrictId { get; set; }
        public int? CityId { get; set; }
        public string? AvatarUrl { get; set; }

        // TutorProfile properties
        public string? Bio { get; set; }
        public string? TeachingExp { get; set; }
        public string? VideoIntroUrl { get; set; }
        public TeachingMode? TeachingModes { get; set; }
    }
}