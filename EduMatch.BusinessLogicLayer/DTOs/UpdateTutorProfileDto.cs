namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class UpdateTutorProfileDto
    {
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public string? TeachingExp { get; set; }
        public string? VideoIntroUrl { get; set; }
        public string? TeachingModes { get; set; }
    }
}
