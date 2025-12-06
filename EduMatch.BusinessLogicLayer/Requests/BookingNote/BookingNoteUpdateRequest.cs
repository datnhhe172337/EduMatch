using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.BookingNote
{
    public class BookingNoteUpdateRequest
    {
        [Required(ErrorMessage = "Id là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Id phải lớn hơn 0")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Content là bắt buộc")]
        [MaxLength(2000, ErrorMessage = "Content không được vượt quá 2000 ký tự")]
        public string Content { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "ImageUrl không được vượt quá 500 ký tự")]
        public string? ImageUrl { get; set; }

        [MaxLength(255, ErrorMessage = "ImagePublicId không được vượt quá 255 ký tự")]
        public string? ImagePublicId { get; set; }

        [MaxLength(500, ErrorMessage = "VideoUrl không được vượt quá 500 ký tự")]
        public string? VideoUrl { get; set; }

        [MaxLength(255, ErrorMessage = "VideoPublicId không được vượt quá 255 ký tự")]
        public string? VideoPublicId { get; set; }
    }
}
