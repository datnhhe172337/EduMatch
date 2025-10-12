namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class UpdateUserProfileDto
    {
        public string? AvatarUrl { get; set; }
        public int? CityId { get; set; }
        public int? SubDistrictId { get; set; }
        public string? AddressLine { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
