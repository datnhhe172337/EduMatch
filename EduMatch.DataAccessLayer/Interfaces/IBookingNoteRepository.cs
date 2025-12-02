using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IBookingNoteRepository
    {
        Task<BookingNote?> GetByIdAsync(int id);
        Task<List<BookingNote>> GetByBookingIdAsync(int bookingId);
        Task<BookingNote> CreateAsync(BookingNote entity);
        Task<BookingNote?> UpdateAsync(BookingNote entity);
        Task<bool> DeleteAsync(int id);
    }
}
