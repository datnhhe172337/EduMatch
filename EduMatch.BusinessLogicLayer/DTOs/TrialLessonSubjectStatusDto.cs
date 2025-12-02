namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class TrialLessonSubjectStatusDto
    {
        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public bool HasTrialed { get; set; }
    }
}
