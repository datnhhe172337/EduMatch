using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class CertificateTypeDto
	{
		public int Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public DateTime? CreatedAt { get; set; }
	}
}
