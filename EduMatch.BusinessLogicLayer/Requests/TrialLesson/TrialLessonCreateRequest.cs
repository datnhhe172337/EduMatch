using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TrialLesson
{
    public class TrialLessonCreateRequest
    {
        [Required]
        public int TutorId { get; set; }

        [Required]
        public int SubjectId { get; set; }
    }
}
