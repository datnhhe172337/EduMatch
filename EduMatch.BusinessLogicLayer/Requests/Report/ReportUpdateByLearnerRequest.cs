using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class ReportUpdateByLearnerRequest
    {
        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;
    }
}
