using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class RefreshTokenDto
	{
		public int Id { get; set; }
		public string Token { get; set; } = null!;
		public DateTime ExpiryDate { get; set; }
		public string UserEmail { get; set; } = null!;
		public UserDto? User { get; set; }
	}
}
