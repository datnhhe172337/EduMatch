using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Level
{
	public class LevelUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Level name is required")]
		[StringLength(50, ErrorMessage = "Level name cannot exceed 50 characters")]
		public string Name { get; set; } = null!;
	}
}
