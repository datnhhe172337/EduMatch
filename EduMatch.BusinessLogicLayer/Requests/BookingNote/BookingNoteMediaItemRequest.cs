using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.BookingNote
{
    public class BookingNoteMediaItemRequest
    {
        [Required(ErrorMessage = "MediaType là bắt buộc")]
        public MediaType MediaType { get; set; }

        [Required(ErrorMessage = "FileUrl là bắt buộc")]
        [MaxLength(500, ErrorMessage = "FileUrl không được vượt quá 500 ký tự")]
        public string FileUrl { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "FilePublicId không được vượt quá 255 ký tự")]
        public string? FilePublicId { get; set; }
    }
}
