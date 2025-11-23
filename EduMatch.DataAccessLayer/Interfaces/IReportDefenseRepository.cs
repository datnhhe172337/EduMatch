using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IReportDefenseRepository
    {
        Task<ReportDefense> AddAsync(ReportDefense defense);
        Task<IReadOnlyList<ReportDefense>> GetByReportIdAsync(int reportId);
        Task<ReportDefense?> GetByIdAsync(int id);
        Task<ReportDefense> UpdateAsync(ReportDefense defense);
        Task DeleteAsync(ReportDefense defense);
    }
}
