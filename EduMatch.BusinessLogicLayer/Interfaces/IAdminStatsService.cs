using EduMatch.BusinessLogicLayer.DTOs;
using System;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IAdminStatsService
    {
        Task<AdminSummaryStatsDto> GetSummaryAsync(DateTime? fromUtc = null, DateTime? toUtc = null);
    }
}
