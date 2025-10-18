using System;
using System.Text.Json.Serialization;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorSubjectDto
	{
		public int Id { get; set; }

		[JsonIgnore]
		public int TutorId { get; set; }
		[JsonIgnore]
		public int SubjectId { get; set; }
		public decimal? HourlyRate { get; set; }
		[JsonIgnore]
		public int? LevelId { get; set; }
		public LevelDto? Level { get; set; }
		public SubjectDto? Subject { get; set; }
		public TutorProfileDto? Tutor { get; set; }
	}
}
