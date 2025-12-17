using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IReportRepository
    {
        Task<Report> CreateAsync(Report report);
        Task<Report?> GetByIdAsync(int id);
        Task<List<Report>> GetByReporterAsync(string reporterEmail);
        Task<List<Report>> GetByReportedUserAsync(string reportedEmail);
        Task<List<Report>> GetByBookingIdAsync(int bookingId);
        Task<List<Report>> GetByScheduleIdAsync(int scheduleId);
        Task<List<Report>> GetAllAsync();
        Task<Report> UpdateAsync(Report report);
        Task DeleteAsync(Report report);
        Task<bool> ExistsSimilarReportAsync(string reporterEmail, string reportedEmail, string reason, int? excludeReportId = null, int lookbackDays = 7);
    }
}
