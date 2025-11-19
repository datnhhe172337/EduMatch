using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IReportContentValidator
    {
        Task ValidateAsync(string reporterEmail, string reportedEmail, string reason, int? currentReportId = null);
    }
}
