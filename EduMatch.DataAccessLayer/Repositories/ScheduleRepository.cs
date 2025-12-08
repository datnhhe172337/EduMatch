using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduMatch.DataAccessLayer.Enum;    

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly EduMatchContext _context;
        public ScheduleRepository(EduMatchContext context) => _context = context;

        /// <summary>
        /// Lấy danh sách Schedule theo BookingId và Status với phân trang
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page, int pageSize)
        {
            var query = _context.Schedules
            .AsSplitQuery()
            .Include(s => s.Availabiliti)
				.ThenInclude(s => s.Slot)
			.Include(s => s.Booking)
            .Include(s => s.MeetingSession)
            .Include(s => s.ScheduleCompletion)
            .Include(s => s.TutorPayout)
            .Where(s => s.BookingId == bookingId);
            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);
            return await query
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        /// <summary>
        /// Lấy danh sách Schedule theo BookingId và Status (không phân trang)
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetAllByBookingIdAndStatusNoPagingAsync(int bookingId, int? status)
        {
            var query = _context.Schedules
            .AsSplitQuery()
            .Include(s => s.Availabiliti)
                .ThenInclude(s =>s .Slot)
            .Include(s => s.Booking)
            .Include(s => s.MeetingSession)
            .Include(s => s.ScheduleCompletion)
            .Include(s => s.TutorPayout)
            .Where(s => s.BookingId == bookingId);
            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);
            return await query
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .ToListAsync();
        }
        /// <summary>
        /// Đếm số lượng Schedule theo BookingId và Status
        /// </summary>
        public async Task<int> CountByBookingIdAndStatusAsync(int bookingId, int? status)
        {
            var query = _context.Schedules.AsSplitQuery()
            .Include(s => s.Availabiliti)
            .Include(s => s.Booking)
            .Where(s => s.BookingId == bookingId);
            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);
            return await query.CountAsync();
        }
        /// <summary>
        /// Lấy Schedule theo AvailabilityId
        /// </summary>
        public async Task<Schedule?> GetByAvailabilityIdAsync(int availabilitiId)
        {
            return await _context.Schedules
                .Include(s => s.MeetingSession)
                .Include(s => s.ScheduleCompletion)
                .Include(s => s.TutorPayout)
                .FirstOrDefaultAsync(s => s.AvailabilitiId == availabilitiId);
        }
        /// <summary>
        /// Lấy Schedule theo ID
        /// </summary>
        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _context.Schedules
                .Include(s => s.Availabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Tutor)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Subject)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Level)
                .Include(s => s.MeetingSession)
                .Include(s => s.ScheduleCompletion)
                .Include(s => s.TutorPayout)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        /// <summary>
        /// Lấy danh sách Schedule theo BookingId đã sắp xếp
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetAllByBookingIdOrderedAsync(int bookingId)
        {
            return await _context.Schedules
                .Where(s => s.BookingId == bookingId)
                .Include(s => s.MeetingSession)
                .Include(s => s.ScheduleCompletion)
                .Include(s => s.TutorPayout)
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .ToListAsync();
        }
        /// <summary>
        /// Lấy danh sách Schedule theo LearnerEmail (qua Booking) và optional khoảng thời gian (qua TutorAvailability.StartDate) và Status
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetAllByLearnerEmailAsync(string learnerEmail, DateTime? startDate = null, DateTime? endDate = null, int? status = null)
        {
            var query = _context.Schedules
                .AsSplitQuery()
                .Include(s => s.Availabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Tutor)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Subject)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Level)
                .Include(s => s.MeetingSession)
                .Include(s => s.ScheduleCompletion)
                .Include(s => s.TutorPayout)
                .Where(s => s.Booking.LearnerEmail == learnerEmail);

            if (startDate.HasValue)
            {
                var startDateOnly = startDate.Value.Date;
                query = query.Where(s => s.Availabiliti.StartDate.Date >= startDateOnly);
            }

            if (endDate.HasValue)
            {
                var endDateOnly = endDate.Value.Date;
                query = query.Where(s => s.Availabiliti.StartDate.Date <= endDateOnly);
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .ToListAsync();
        }
        /// <summary>
        /// Lấy danh sách Schedule theo TutorEmail (qua TutorProfile -> TutorSubject -> Booking) và optional khoảng thời gian và Status
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetAllByTutorEmailAsync(string tutorEmail, DateTime? startDate = null, DateTime? endDate = null, int? status = null)
        {
            var query = _context.Schedules
                .AsSplitQuery()
                .Include(s => s.Availabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Tutor)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Subject)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Level)
                .Include(s => s.MeetingSession)
                .Include(s => s.ScheduleCompletion)
                .Include(s => s.TutorPayout)
                .Where(s => s.Booking.TutorSubject.Tutor.UserEmail == tutorEmail);

            if (startDate.HasValue)
            {
                var startDateOnly = startDate.Value.Date;
                query = query.Where(s => s.Availabiliti.StartDate.Date >= startDateOnly);
            }

            if (endDate.HasValue)
            {
                var endDateOnly = endDate.Value.Date;
                query = query.Where(s => s.Availabiliti.StartDate.Date <= endDateOnly);
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .ToListAsync();
        }
        /// <summary>
        /// Kiểm tra tutor có lịch học trùng với slot và ngày hay không (loại trừ Schedule bị Cancelled)
        /// </summary>
        public async Task<bool> HasTutorScheduleOnSlotDateAsync(int tutorId, int slotId, DateTime date)
        {
            var dateOnly = date.Date;
            return await _context.Schedules
                .Include(s => s.Availabiliti)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                .AnyAsync(s => s.Booking.TutorSubject.TutorId == tutorId
                               && s.Availabiliti.SlotId == slotId
                               && s.Availabiliti.StartDate.Date == dateOnly
                               && s.Status != (int)ScheduleStatus.Cancelled);
        }
        /// <summary>
        /// Tạo Schedule mới
        /// </summary>
        public async Task CreateAsync(Schedule schedule)
        {
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Cập nhật Schedule
        /// </summary>
        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Xóa Schedule theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule != null)
            {
                // Xóa MeetingSession trước (foreign key constraint)
                var meetingSession = await _context.MeetingSessions
                    .FirstOrDefaultAsync(ms => ms.ScheduleId == id);
                if (meetingSession != null)
                {
                    _context.MeetingSessions.Remove(meetingSession);
                }

                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Lấy tất cả Schedule có status Upcoming hoặc InProgress với đầy đủ thông tin Availability và Slot
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetAllUpcomingAndInProgressAsync()
        {
            return await _context.Schedules
                .AsSplitQuery()
                .Include(s => s.Availabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(s => s.Booking)
                .Include(s => s.MeetingSession)
                .Include(s => s.ScheduleCompletion)
                .Include(s => s.TutorPayout)
                .Where(s => s.Status == (int)ScheduleStatus.Upcoming || s.Status == (int)ScheduleStatus.InProgress)
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .ToListAsync();
        }
    }
}
