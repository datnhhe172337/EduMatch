using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Subject
{
	public class SubjectCreateRequest
	{
		[Required(ErrorMessage = "Subject name is required")]
		[StringLength(100, ErrorMessage = "Subject name cannot exceed 100 characters")]
		public string SubjectName { get; set; } = null!;
	}
}