using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ReportContentValidator : IReportContentValidator
    {
        private readonly IReportRepository _reportRepository;
        private const int MinReasonLength = 10;
        private const int MaxReasonLength = 1000;
        private const int DuplicateLookbackDays = 7;

        public ReportContentValidator(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task ValidateAsync(string reporterEmail, string reportedEmail, string reason, int? currentReportId = null)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Lý do là bắt buộc.");

            var trimmed = reason.Trim();
            if (trimmed.Length < MinReasonLength || trimmed.Length > MaxReasonLength)
                throw new ArgumentException($"Lý do phải có độ dài từ {MinReasonLength} đến {MaxReasonLength} ký tự.");

            var isDuplicate = await _reportRepository.ExistsSimilarReportAsync(
                reporterEmail.Trim(),
                reportedEmail.Trim(),
                trimmed,
                currentReportId,
                DuplicateLookbackDays);

            if (isDuplicate)
                throw new InvalidOperationException("Bạn đã gửi một báo cáo tương tự gần đây. Vui lòng chờ thêm trước khi gửi lại cùng nội dung.");
        }
    }
}
