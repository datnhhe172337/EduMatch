using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
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
                        .ThenInclude(t => t.UserEmailNavigation)
                            .ThenInclude(u => u.UserProfile)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.MeetingSession)
                .AsQueryable();
            if (!string.IsNullOrEmpty(email))
                query = query.Where(b => b.LearnerEmail == email);
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách bookings theo learnerEmail (không phân trang) với lọc theo status, tutorSubjectId
        /// </summary>
        public async Task<IEnumerable<Booking>> GetAllByLearnerEmailNoPagingAsync(string email, int? status, int? tutorSubjectId)
        {
            var query = _context.Bookings
                .AsSplitQuery()
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Level)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Tutor)
                        .ThenInclude(t => t.UserEmailNavigation)
                            .ThenInclude(u => u.UserProfile)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.MeetingSession)
                .AsQueryable();
            if (!string.IsNullOrEmpty(email))
                query = query.Where(b => b.LearnerEmail == email);
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .ToListAsync();
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
           var query = _context.Bookings
                .AsSplitQuery()
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Level)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Tutor)
                        .ThenInclude(t => t.UserEmailNavigation)
                            .ThenInclude(u => u.UserProfile)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.MeetingSession)
                .Where(b => b.TutorSubject.TutorId == tutorId)   
                .AsQueryable();
                
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách bookings theo tutorId (không phân trang) với lọc theo status, tutorSubjectId
        /// </summary>
        public async Task<IEnumerable<Booking>> GetAllByTutorIdNoPagingAsync(int tutorId, int? status, int? tutorSubjectId)
        {
            var query = _context.Bookings
                .AsSplitQuery()
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Level)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Tutor)
                        .ThenInclude(t => t.UserEmailNavigation)
                            .ThenInclude(u => u.UserProfile)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.MeetingSession)
                .Where(b => b.TutorSubject.TutorId == tutorId)
                .AsQueryable();
            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);
            if (tutorSubjectId.HasValue)
                query = query.Where(b => b.TutorSubjectId == tutorSubjectId.Value);
            return await query
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .ToListAsync();
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
                        .ThenInclude(t => t.UserEmailNavigation)
                            .ThenInclude(u => u.UserProfile)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.MeetingSession)
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
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

        /// <summary>
        /// Lấy danh sách booking cần auto-cancel nếu quá hạn xác nhận/thanh toán hoặc lịch học sắp diễn ra
        /// </summary>
        public async Task<List<Booking>> GetPendingBookingsNeedingAutoCancelAsync(DateTime createdBefore, DateTime scheduleStartBefore)
        {
            return await _context.Bookings
                .AsSplitQuery()
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Availabiliti)
                .Where(b =>
                    (
                        b.Status == (int)BookingStatus.Pending ||
                        (b.Status == (int)BookingStatus.Confirmed && b.PaymentStatus == (int)PaymentStatus.Pending)
                    ) &&
                    (
                        b.CreatedAt <= createdBefore ||
                        b.Schedules.Any(s => s.Availabiliti.StartDate <= scheduleStartBefore)
                    ))
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách Booking có status Confirmed
        /// </summary>
        public async Task<List<Booking>> GetConfirmedBookingsAsync()
        {
            return await _context.Bookings
                .Where(b => b.Status == (int)BookingStatus.Confirmed)
                .ToListAsync();
        }
    }
}
