using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class ReportDefenseUpdateRequest
    {
        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Note { get; set; } = string.Empty;
    }
}
