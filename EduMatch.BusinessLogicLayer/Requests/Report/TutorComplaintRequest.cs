using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class TutorComplaintRequest
    {
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string DefenseNote { get; set; } = string.Empty;
    }
}
