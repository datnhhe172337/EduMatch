using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class ReportUpdateRequest
    {
        [Required]
        public ReportStatus Status { get; set; }

        [StringLength(1000)]
        public string? AdminNotes { get; set; }
    }
}
