using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Enum;
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
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
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
        public async Task<IEnumerable<ScheduleChangeRequest>> GetAllByRequesterEmailAsync(string requesterEmail, int? status = null)
        {
            var query = _context.ScheduleChangeRequests
                .AsSplitQuery()
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Where(scr => scr.RequesterEmail == requesterEmail);

            if (status.HasValue)
            {
                query = query.Where(scr => scr.Status == status.Value);
            }

            return await query
                .OrderByDescending(scr => scr.CreatedAt)
                .ThenByDescending(scr => scr.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequestedToEmail, sắp xếp theo CreatedAt descending, Id descending
        /// </summary>
        public async Task<IEnumerable<ScheduleChangeRequest>> GetAllByRequestedToEmailAsync(string requestedToEmail, int? status = null)
        {
            var query = _context.ScheduleChangeRequests
                .AsSplitQuery()
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Where(scr => scr.RequestedToEmail == requestedToEmail);

            if (status.HasValue)
            {
                query = query.Where(scr => scr.Status == status.Value);
            }

            return await query
                .OrderByDescending(scr => scr.CreatedAt)
                .ThenByDescending(scr => scr.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo ScheduleId (có thể lọc thêm status)
        /// </summary>
        public async Task<IEnumerable<ScheduleChangeRequest>> GetAllByScheduleIdAsync(int scheduleId, int? status = null)
        {
            var query = _context.ScheduleChangeRequests
                .AsSplitQuery()
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Where(scr => scr.ScheduleId == scheduleId);

            if (status.HasValue)
            {
                query = query.Where(scr => scr.Status == status.Value);
            }

            return await query
                .OrderByDescending(scr => scr.CreatedAt)
                .ThenByDescending(scr => scr.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả ScheduleChangeRequest có Status = Pending với đầy đủ thông tin liên quan
        /// </summary>
        public async Task<IEnumerable<ScheduleChangeRequest>> GetAllPendingAsync()
        {
            return await _context.ScheduleChangeRequests
                .AsSplitQuery()
                .Include(scr => scr.Schedule)
                    .ThenInclude(s => s.Availabiliti)
                        .ThenInclude(a => a.Slot)
                .Include(scr => scr.OldAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(scr => scr.NewAvailabiliti)
                    .ThenInclude(a => a.Slot)
                .Where(scr => scr.Status == (int)EduMatch.DataAccessLayer.Enum.ScheduleChangeRequestStatus.Pending)
                .ToListAsync();
        }
    }
}

