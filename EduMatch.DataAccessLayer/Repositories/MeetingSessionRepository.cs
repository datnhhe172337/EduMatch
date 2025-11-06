using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class MeetingSessionRepository : IMeetingSessionRepository
    {
        private readonly EduMatchContext _context;
        public MeetingSessionRepository(EduMatchContext context) => _context = context;

        /// <summary>
        /// Lấy MeetingSession theo ID
        /// </summary>
        public async Task<MeetingSession?> GetByIdAsync(int id)
        {
            return await _context.MeetingSessions.FirstOrDefaultAsync(ms => ms.Id == id);
        }
        /// <summary>
        /// Lấy MeetingSession theo ScheduleId
        /// </summary>
        public async Task<MeetingSession?> GetByScheduleIdAsync(int scheduleId)
        {
            return await _context.MeetingSessions.FirstOrDefaultAsync(ms => ms.ScheduleId == scheduleId);
        }
        /// <summary>
        /// Tạo MeetingSession mới
        /// </summary>
        public async Task CreateAsync(MeetingSession meetingSession)
        {
            await _context.MeetingSessions.AddAsync(meetingSession);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Cập nhật MeetingSession
        /// </summary>
        public async Task UpdateAsync(MeetingSession meetingSession)
        {
            _context.MeetingSessions.Update(meetingSession);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Xóa MeetingSession theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var ms = await _context.MeetingSessions.FindAsync(id);
            if (ms != null)
            {
                _context.MeetingSessions.Remove(ms);
                await _context.SaveChangesAsync();
            }
        }
    }
}
