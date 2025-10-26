using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class UpdateTutorStatusRequest
	{
		[Required(ErrorMessage = "Status is required")]
		[EnumDataType(typeof(TutorStatus), ErrorMessage = "Invalid TutorStatus")]
		public TutorStatus Status { get; set; }
	}
}
