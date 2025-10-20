using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class RejectTutorRequest
	{
		[MaxLength(500, ErrorMessage = "Reject reason cannot exceed 500 characters.")]
		public string? RejectReason { get; set; }
	}
}
