using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class UpdateTutorStatusRequest
	{
		[Required]
		public bool ApproveAndVerifyAll { get; set; }

		// Required when ApproveAndVerifyAll == false
		public TutorStatus? Status { get; set; }
	}
}
