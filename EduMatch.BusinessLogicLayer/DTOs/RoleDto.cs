using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class RoleDto
	{
		public int Id { get; set; }
		public string RoleName { get; set; } = null!;
		public List<UserDto>? Users { get; set; }
	}
}
