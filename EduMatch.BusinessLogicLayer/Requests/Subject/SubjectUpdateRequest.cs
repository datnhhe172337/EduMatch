using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Subject
{
	public class SubjectUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Subject name is required")]
		[StringLength(100, ErrorMessage = "Subject name cannot exceed 100 characters")]
		public string SubjectName { get; set; } = null!;
	}
}
