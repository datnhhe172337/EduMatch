using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IBookingNoteMediaRepository
    {
        Task<List<BookingNoteMedium>> GetByBookingNoteIdAsync(int bookingNoteId);
        Task ReplaceMediaAsync(int bookingNoteId, IEnumerable<BookingNoteMedium> media);
    }
}
