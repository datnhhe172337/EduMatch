using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class UserProfileDto
	{
		public int Id { get; set; }
		public string UserEmail { get; set; } = null!;
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public Gender? Gender { get; set; }
		public string? AvatarUrl { get; set; }
		public string? AvatarPublicId { get; set; }
		public int? ProvinceId { get; set; }
		public int? SubDistrictId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public UserDto? UserEmailNavigation { get; set; }
		public ProvinceDto? Province { get; set; }
		public SubDistrictDto? SubDistrict { get; set; }
	}
}
