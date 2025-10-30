using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class GoogleTokenDto
	{
		public string AccountEmail { get; set; } = null!;
		public string? TokenType { get; set; }
		public string? Scope { get; set; }
		public DateTime? ExpiresAt { get; set; }
	}
}
