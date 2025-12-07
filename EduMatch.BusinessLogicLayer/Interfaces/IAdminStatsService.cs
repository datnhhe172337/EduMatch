using EduMatch.BusinessLogicLayer.DTOs;
using System;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IAdminStatsService
    {
        Task<AdminSummaryStatsDto> GetSummaryAsync(DateTime? fromUtc = null, DateTime? toUtc = null);
        // Task<IReadOnlyList<SignupTrendPointDto>> GetSignupTrendAsync(DateTime? fromUtc = null, DateTime? toUtc = null, string groupBy = "day");
        // Task<IReadOnlyList<BookingTrendPointDto>> GetBookingTrendAsync(DateTime? fromUtc = null, DateTime? toUtc = null, string groupBy = "day");
        Task<IReadOnlyList<MonthlyAdminStatsDto>> GetMonthlyStatsAsync(int year);
    }
}
