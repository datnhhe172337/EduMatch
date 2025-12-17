using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly EduMatchContext _context;

        public ReportRepository(EduMatchContext context)
        {
            _context = context;
        }

        private IQueryable<Report> BuildQueryable()
        {
            return _context.Reports
                .Include(r => r.ReporterUserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Include(r => r.ReportedUserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Include(r => r.HandledByAdminEmailNavigation)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.SystemFee)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Schedules)
                        .ThenInclude(s => s.MeetingSession)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Subject)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Level)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.MeetingSession)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.ScheduleCompletion)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.TutorPayout)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Availabiliti);
        }

        public async Task<Report> CreateAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(report.Id))!;
        }

        public async Task DeleteAsync(Report report)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Report>> GetAllAsync()
        {
            return await BuildQueryable()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report?> GetByIdAsync(int id)
        {
            return await BuildQueryable()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Report>> GetByReportedUserAsync(string reportedEmail)
        {
            return await BuildQueryable()
                .Where(r => r.ReportedUserEmail == reportedEmail)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Report>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.BookingId == bookingId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Report>> GetByScheduleIdAsync(int scheduleId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.ScheduleId == scheduleId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Report>> GetByReporterAsync(string reporterEmail)
        {
            return await BuildQueryable()
                .Where(r => r.ReporterUserEmail == reporterEmail)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report> UpdateAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(report.Id))!;
        }

        public async Task<bool> ExistsSimilarReportAsync(string reporterEmail, string reportedEmail, string reason, int? excludeReportId = null, int lookbackDays = 7)
        {
            if (string.IsNullOrWhiteSpace(reporterEmail) || string.IsNullOrWhiteSpace(reportedEmail) || string.IsNullOrWhiteSpace(reason))
                return false;

            var normalizedReason = reason.Trim();
            var cutoff = DateTime.UtcNow.AddDays(-lookbackDays);

            var query = _context.Reports.AsQueryable();

            if (excludeReportId.HasValue)
                query = query.Where(r => r.Id != excludeReportId.Value);

            return await query.AnyAsync(r =>
                r.ReporterUserEmail == reporterEmail &&
                r.ReportedUserEmail == reportedEmail &&
                r.Reason == normalizedReason &&
                r.CreatedAt >= cutoff);
        }
    }
}
