using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class ApproveTutorRequest
	{
		[Required]
		public bool VerifyAll { get; set; }
	}
}
