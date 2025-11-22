using System;
using System.Text.Json.Serialization;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorSubjectDto
	{
		public int Id { get; set; }

		public int TutorId { get; set; }
		public string TutorEmail { get; set; }
		[JsonIgnore]
		public int SubjectId { get; set; }
		public decimal? HourlyRate { get; set; }
		[JsonIgnore]
		public int? LevelId { get; set; }
		public LevelDto? Level { get; set; }
		public SubjectDto? Subject { get; set; }
		[JsonIgnore]
		public TutorProfileDto? Tutor { get; set; }
	}
}
