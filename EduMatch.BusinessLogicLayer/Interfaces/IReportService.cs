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
        Task<IReadOnlyList<int>> GetReportIdsByBookingIdAsync(int bookingId, string? currentUserEmail, bool currentUserIsAdmin);
        Task<IReadOnlyList<int>> GetReportIdsByScheduleIdAsync(int scheduleId, string? currentUserEmail, bool currentUserIsAdmin);
        Task<ReportDetailDto?> GetReportDetailAsync(int reportId, string requesterEmail, bool requesterIsAdmin);
        Task<ReportDetailDto> UpdateReportAsync(int reportId, ReportUpdateRequest request, string adminEmail);
        Task<ReportDetailDto> UpdateReportByLearnerAsync(int reportId, ReportUpdateByLearnerRequest request, string learnerEmail);
        Task<ReportDetailDto> CancelReportByLearnerAsync(int reportId, string learnerEmail);
        Task DeleteReportAsync(int reportId);
        Task<ReportEvidenceDto> AddEvidenceAsync(int reportId, ReportEvidenceCreateRequest request, string currentUserEmail, bool currentUserIsAdmin);
        Task<IReadOnlyList<ReportEvidenceDto>> GetEvidenceByReportIdAsync(int reportId, string currentUserEmail, bool currentUserIsAdmin);
        Task<ReportEvidenceDto> UpdateEvidenceAsync(int reportId, int evidenceId, ReportEvidenceUpdateRequest request, string currentUserEmail, bool currentUserIsAdmin);
        Task DeleteEvidenceAsync(int reportId, int evidenceId, string currentUserEmail, bool currentUserIsAdmin);
        Task<IReadOnlyList<ReportListItemDto>> GetAllReportsAsync();
        Task<ReportDefenseDto> AddDefenseAsync(int reportId, ReportDefenseCreateRequest request, string tutorEmail, bool currentUserIsAdmin);
        Task<IReadOnlyList<ReportDefenseDto>> GetDefensesAsync(int reportId, string currentUserEmail, bool currentUserIsAdmin);
        Task<ReportDefenseDto> UpdateDefenseAsync(int reportId, int defenseId, ReportDefenseUpdateRequest request, string currentUserEmail, bool currentUserIsAdmin);
        Task DeleteDefenseAsync(int reportId, int defenseId, string currentUserEmail, bool currentUserIsAdmin);
        Task<ReportFullDetailDto?> GetFullReportDetailAsync(int reportId, string requesterEmail, bool requesterIsAdmin);
        Task<bool> CanSubmitDefenseAsync(int reportId, string tutorEmail, bool currentUserIsAdmin);
        Task<bool> IsReportResolvedAsync(int reportId, string requesterEmail, bool requesterIsAdmin);
    }
}
