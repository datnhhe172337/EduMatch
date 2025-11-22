using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IReportEvidenceRepository
    {
        Task<ReportEvidence> AddAsync(ReportEvidence evidence);
        Task<IReadOnlyList<ReportEvidence>> GetByReportIdAsync(int reportId);
        Task<ReportEvidence?> GetByIdAsync(int evidenceId);
        Task<ReportEvidence> UpdateAsync(ReportEvidence evidence);
        Task DeleteAsync(ReportEvidence evidence);
    }
}
