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

        public List<BookingNoteMediaItemRequest>? Media { get; set; }
    }
}
