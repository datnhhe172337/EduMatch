using System;
using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class UserDto
	{
		public string Email { get; set; } = null!;
		public string? UserName { get; set; }
		public string? PasswordHash { get; set; }
		public string? Phone { get; set; }
		public bool? IsEmailConfirmed { get; set; }
		public string LoginProvider { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public bool? IsActive { get; set; }
		public int RoleId { get; set; }
		public List<RefreshTokenDto>? RefreshTokens { get; set; }
		public RoleDto? Role { get; set; }
		public TutorProfileDto? TutorProfile { get; set; }
		public UserProfileDto? UserProfile { get; set; }
	}
}
