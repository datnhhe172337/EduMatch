using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class RefundPolicyDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; }
		public decimal RefundPercentage { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; } = null!;
		public DateTime? UpdatedAt { get; set; }
		public string? UpdatedBy { get; set; }
	}
}

