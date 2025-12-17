using EduMatch.BusinessLogicLayer.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorDashboardService
    {
        Task<IReadOnlyList<ScheduleDto>> GetUpcomingLessonsAsync(string tutorEmail);
        Task<IReadOnlyList<ScheduleDto>> GetTodaySchedulesAsync(string tutorEmail);
        Task<IReadOnlyList<BookingDto>> GetPendingBookingsAsync(string tutorEmail);
        Task<IReadOnlyList<TutorMonthlyEarningDto>> GetMonthlyEarningsAsync(string tutorEmail, int year);
        Task<TutorMonthlyEarningDto> GetCurrentMonthEarningAsync(string tutorEmail);
        Task<IReadOnlyList<ReportListItemDto>> GetReportsPendingDefenseAsync(string tutorEmail);
    }
}
