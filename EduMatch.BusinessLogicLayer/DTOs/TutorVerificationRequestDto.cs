using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorVerificationRequestDto
	{
		public int Id { get; set; }
		public string UserEmail { get; set; } = null!;
		public int? TutorId { get; set; }
		public TutorVerificationRequestStatus Status { get; set; }
		public string? Description { get; set; }
		public string? AdminNote { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? ProcessedBy { get; set; }
		public DateTime CreatedAt { get; set; }

		// Optional nested objects
		public TutorProfileDto? Tutor { get; set; }
		public UserDto? User { get; set; }
	}
}

