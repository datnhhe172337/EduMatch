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

        public async Task<MeetingSession?> GetByIdAsync(int id)
        {
            return await _context.MeetingSessions.FirstOrDefaultAsync(ms => ms.Id == id);
        }
        public async Task<IEnumerable<MeetingSession>> GetByScheduleIdAsync(int scheduleId)
        {
            return await _context.MeetingSessions.Where(ms => ms.ScheduleId == scheduleId).ToListAsync();
        }
        public async Task CreateAsync(MeetingSession meetingSession)
        {
            await _context.MeetingSessions.AddAsync(meetingSession);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(MeetingSession meetingSession)
        {
            _context.MeetingSessions.Update(meetingSession);
            await _context.SaveChangesAsync();
        }
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
