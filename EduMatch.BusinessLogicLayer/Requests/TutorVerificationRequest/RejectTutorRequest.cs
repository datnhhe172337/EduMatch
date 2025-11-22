using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest
{
	public class RejectTutorRequest
	{
		[Required(ErrorMessage = "Reason là bắt buộc")]
		[MaxLength(1000, ErrorMessage = "Reason không được vượt quá 1000 ký tự")]
		public string Reason { get; set; } = null!;
	}
}

