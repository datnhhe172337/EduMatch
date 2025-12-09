using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class BookingNoteDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Content { get; set; } = null!;
        public string? CreatedByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BookingNoteMediaDto>? Media { get; set; }
    }
}
