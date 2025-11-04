using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly EduMatchContext _context;
        public BookingRepository(EduMatchContext context) => _context = context;

        public async Task<IEnumerable<Booking>> GetAllByLearnerEmailAsync(string email, int? status, int? tutorSubjectId, int page, int pageSize)
        {
            var query = _context.Bookings.AsQueryable();
            if (!string.IsNullOrEmpty(email))
                query = query.Where(b => b.LearnerEmail == email);
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        public async Task<int> CountByLearnerEmailAsync(string email, int? status, int? tutorSubjectId)
        {
            var query = _context.Bookings.AsQueryable();
            if (!string.IsNullOrEmpty(email))
                query = query.Where(b => b.LearnerEmail == email);
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query.CountAsync();
        }
        public async Task<IEnumerable<Booking>> GetAllByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId, int page, int pageSize)
        {
           var query =
                        from b in _context.Bookings
                        join ts in _context.TutorSubjects on b.TutorSubjectId equals ts.Id
                        where ts.TutorId == tutorId
                        select b;
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        public async Task<int> CountByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId)
        {
            var query =
                from b in _context.Bookings
                join ts in _context.TutorSubjects on b.TutorSubjectId equals ts.Id
                where ts.TutorId == tutorId
                select b;
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query.CountAsync();
        }
        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        }
        public async Task CreateAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }
    }
}
