using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.BookingNote;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IBookingNoteService
    {
        Task<BookingNoteDto?> GetByIdAsync(int id);
        Task<List<BookingNoteDto>> GetByBookingIdAsync(int bookingId);
        Task<BookingNoteDto> CreateAsync(BookingNoteCreateRequest request);
        Task<BookingNoteDto?> UpdateAsync(BookingNoteUpdateRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
