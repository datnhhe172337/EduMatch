using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class BookingNoteMediaDto
    {
        public int Id { get; set; }
        public int BookingNoteId { get; set; }
        public MediaType MediaType { get; set; }
        public string FileUrl { get; set; } = null!;
        public string? FilePublicId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
