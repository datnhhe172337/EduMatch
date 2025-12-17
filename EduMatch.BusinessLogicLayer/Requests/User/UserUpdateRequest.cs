using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.User
{
	public class UserUpdateRequest
	{
		[Required, EmailAddress(ErrorMessage = "Invalid email address")]
		public string Email { get; set; } = null!;

		[StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
		public string? UserName { get; set; }

		public string? Phone { get; set; }

		public bool? IsEmailConfirmed { get; set; }

		public bool? IsActive { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Role ID must be greater than 0")]
		public int RoleId { get; set; }
	}
}
