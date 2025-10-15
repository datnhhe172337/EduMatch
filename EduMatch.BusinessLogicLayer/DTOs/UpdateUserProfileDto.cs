using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class UpdateUserProfileDto
    {
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public DateTime? Dob { get; set; }
        public Gender? Gender { get; set; }

        public string? AvatarUrl { get; set; }
        public string? AvatarUrlPublicId { get; set; }

        public int? CityId { get; set; }
        public int? SubDistrictId { get; set; }

        public string? AddressLine { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
