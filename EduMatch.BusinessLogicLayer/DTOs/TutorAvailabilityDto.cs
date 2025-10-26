using EduMatch.DataAccessLayer.Enum;
using System;
using System.Text.Json.Serialization;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorAvailabilityDto
	{
		public int Id { get; set; }
		[JsonIgnore]
		public int TutorId { get; set; }
		[JsonIgnore]
		public int SlotId { get; set; }
		public TutorAvailabilityStatus? Status { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public TimeSlotDto? Slot { get; set; }
		[JsonIgnore]
		public TutorProfileDto? Tutor { get; set; }
	}
}
