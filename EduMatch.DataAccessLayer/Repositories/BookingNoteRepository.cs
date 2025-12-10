using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class BookingNoteRepository : IBookingNoteRepository
    {
        private readonly EduMatchContext _context;

        public BookingNoteRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<BookingNote?> GetByIdAsync(int id)
        {
            return await _context.BookingNotes.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<BookingNote>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.BookingNotes
                .Where(n => n.BookingId == bookingId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<BookingNote> CreateAsync(BookingNote entity)
        {
            await _context.BookingNotes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<BookingNote?> UpdateAsync(BookingNote entity)
        {
            var existing = await _context.BookingNotes.FirstOrDefaultAsync(n => n.Id == entity.Id);
            if (existing == null)
                return null;

            existing.Content = entity.Content;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.BookingNotes.FirstOrDefaultAsync(n => n.Id == id);
            if (entity == null)
                return false;

            _context.BookingNotes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
