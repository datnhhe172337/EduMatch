using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class BookingNoteMediaRepository : IBookingNoteMediaRepository
    {
        private readonly EduMatchContext _context;

        public BookingNoteMediaRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<List<BookingNoteMedium>> GetByBookingNoteIdAsync(int bookingNoteId)
        {
            return await _context.BookingNoteMedia
                .Where(m => m.BookingNoteId == bookingNoteId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task ReplaceMediaAsync(int bookingNoteId, IEnumerable<BookingNoteMedium> media)
        {
            var existing = _context.BookingNoteMedia.Where(m => m.BookingNoteId == bookingNoteId);
            _context.BookingNoteMedia.RemoveRange(existing);
            if (media.Any())
            {
                await _context.BookingNoteMedia.AddRangeAsync(media);
            }
            await _context.SaveChangesAsync();
        }
    }
}
