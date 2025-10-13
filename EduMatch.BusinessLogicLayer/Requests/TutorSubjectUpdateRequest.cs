using System;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorSubjectUpdateRequest
	{
		public int Id { get; set; }
		public int TutorId { get; set; }
		public int SubjectId { get; set; }
		public decimal? HourlyRate { get; set; }
		public int? LevelId { get; set; }
	}
}
