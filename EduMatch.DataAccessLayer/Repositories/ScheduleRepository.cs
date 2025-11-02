using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly EduMatchContext _context;
        public ScheduleRepository(EduMatchContext context) => _context = context;

        public async Task<IEnumerable<Schedule>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page, int pageSize)
        {
            var query = _context.Schedules
            .AsSplitQuery()
            .Include(s => s.Availabiliti)
            .Include(s => s.Booking)
            .Where(s => s.BookingId == bookingId);
            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
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
        public async Task<Schedule?> GetByAvailabilityIdAsync(int availabilitiId)
        {
            return await _context.Schedules.FirstOrDefaultAsync(s => s.AvailabilitiId == availabilitiId);
        }
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
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task CreateAsync(Schedule schedule)
        {
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }
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
    }
}
