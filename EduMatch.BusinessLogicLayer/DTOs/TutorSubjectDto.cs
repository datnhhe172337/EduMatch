using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorSubjectDto
	{
		public int Id { get; set; }
		public int TutorId { get; set; }
		public int SubjectId { get; set; }
		public decimal? HourlyRate { get; set; }
		public int? LevelId { get; set; }
		public LevelDto? Level { get; set; }
		public SubjectDto? Subject { get; set; }
		public TutorProfileDto? Tutor { get; set; }
	}
}
