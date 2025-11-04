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

        /// <summary>
        /// Lấy danh sách bookings theo learnerEmail với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        public async Task<IEnumerable<Booking>> GetAllByLearnerEmailAsync(string email, int? status, int? tutorSubjectId, int page, int pageSize)
        {
            var query = _context.Bookings
                .AsSplitQuery()
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Level)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Tutor)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .AsQueryable();
            if (!string.IsNullOrEmpty(email))
                query = query.Where(b => b.LearnerEmail == email);
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        /// <summary>
        /// Đếm tổng số bookings theo learnerEmail với lọc theo status, tutorSubjectId
        /// </summary>
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
        /// <summary>
        /// Lấy danh sách bookings theo tutorId với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        public async Task<IEnumerable<Booking>> GetAllByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId, int page, int pageSize)
        {
            var query =
                from b in _context.Bookings
                    .AsSplitQuery()
                    .Include(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Subject)
                    .Include(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Level)
                    .Include(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Tutor)
                    .Include(b => b.Schedules)
                        .ThenInclude(s => s.Availabiliti)
                            .ThenInclude(a => a.Slot)
                join ts in _context.TutorSubjects on b.TutorSubjectId equals ts.Id
                where ts.TutorId == tutorId
                select b;
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        /// <summary>
        /// Đếm tổng số bookings theo tutorId với lọc theo status, tutorSubjectId
        /// </summary>
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
        /// <summary>
        /// Lấy booking theo id
        /// </summary>
        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .AsSplitQuery()
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Level)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Tutor)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>
        /// Tạo mới booking
        /// </summary>
        public async Task CreateAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật booking
        /// </summary>
        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa booking theo id
        /// </summary>
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
