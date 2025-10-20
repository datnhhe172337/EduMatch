using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class UpdateTutorStatusRequest
	{
		[Required]
		public bool ApproveAndVerifyAll { get; set; }

		// Required when ApproveAndVerifyAll == false
		public EduMatch.DataAccessLayer.Enum.TutorStatus? Status { get; set; }
	}
}
