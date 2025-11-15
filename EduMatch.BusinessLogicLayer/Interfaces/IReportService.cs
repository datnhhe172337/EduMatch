using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IReportService
    {
        Task<ReportDetailDto> CreateReportAsync(ReportCreateRequest request, string reporterEmail);
        Task<IReadOnlyList<ReportListItemDto>> GetReportsByReporterAsync(string reporterEmail);
        Task<IReadOnlyList<ReportListItemDto>> GetReportsByReportedUserAsync(string reportedEmail);
        Task<ReportDetailDto?> GetReportDetailAsync(int reportId, string requesterEmail, bool requesterIsAdmin);
        Task<ReportDetailDto> UpdateReportAsync(int reportId, ReportUpdateRequest request, string adminEmail);
        Task<ReportDetailDto> SubmitTutorComplaintAsync(int reportId, TutorComplaintRequest request, string tutorEmail);
        Task DeleteReportAsync(int reportId);
    }
}
