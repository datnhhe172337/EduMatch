using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Level
{
	public class LevelCreateRequest
	{
		[Required(ErrorMessage = "Level name is required")]
		[StringLength(50, ErrorMessage = "Level name cannot exceed 50 characters")]
		public string Name { get; set; } = null!;
	}
}
