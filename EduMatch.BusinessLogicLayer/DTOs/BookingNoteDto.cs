using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class BookingNoteDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? CreatedByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
