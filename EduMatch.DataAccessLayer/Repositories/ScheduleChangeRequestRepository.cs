using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ScheduleChangeRequestRepository : IScheduleChangeRequestRepository
    {
        private readonly EduMatchContext _context;
        public ScheduleChangeRequestRepository(EduMatchContext context) => _context = context;

        /// <summary>
        /// Lấy ScheduleChangeRequest theo ID với đầy đủ thông tin liên quan
        /// </summary>
        public async Task<ScheduleChangeRequest?> GetByIdAsync(int id)
        {
            return await _context.ScheduleChangeRequests
                .AsSplitQuery()
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Tutor)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Subject)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Level)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.MeetingSession)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Tutor)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Tutor)
                .Include(scr => scr.RequesterEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Include(scr => scr.RequestedToEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .FirstOrDefaultAsync(scr => scr.Id == id);
        }

        /// <summary>
        /// Tạo ScheduleChangeRequest mới
        /// </summary>
        public async Task CreateAsync(ScheduleChangeRequest scheduleChangeRequest)
        {
            await _context.ScheduleChangeRequests.AddAsync(scheduleChangeRequest);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật ScheduleChangeRequest
        /// </summary>
        public async Task UpdateAsync(ScheduleChangeRequest scheduleChangeRequest)
        {
            _context.ScheduleChangeRequests.Update(scheduleChangeRequest);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequesterEmail, sắp xếp theo CreatedAt descending, Id descending
        /// </summary>
        public async Task<IEnumerable<ScheduleChangeRequest>> GetAllByRequesterEmailAsync(string requesterEmail)
        {
            return await _context.ScheduleChangeRequests
                .AsSplitQuery()
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Tutor)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Subject)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Level)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.MeetingSession)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Tutor)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Tutor)
                .Include(scr => scr.RequesterEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Include(scr => scr.RequestedToEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Where(scr => scr.RequesterEmail == requesterEmail)
                .OrderByDescending(scr => scr.CreatedAt)
                .ThenByDescending(scr => scr.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequestedToEmail, sắp xếp theo CreatedAt descending, Id descending
        /// </summary>
        public async Task<IEnumerable<ScheduleChangeRequest>> GetAllByRequestedToEmailAsync(string requestedToEmail)
        {
            return await _context.ScheduleChangeRequests
                .AsSplitQuery()
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Tutor)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Subject)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Booking)
                        .ThenInclude(b => b.TutorSubject)
                            .ThenInclude(ts => ts.Level)
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.MeetingSession)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Tutor)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Tutor)
                .Include(scr => scr.RequesterEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Include(scr => scr.RequestedToEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Where(scr => scr.RequestedToEmail == requestedToEmail)
                .OrderByDescending(scr => scr.CreatedAt)
                .ThenByDescending(scr => scr.Id)
                .ToListAsync();
        }
    }
}

