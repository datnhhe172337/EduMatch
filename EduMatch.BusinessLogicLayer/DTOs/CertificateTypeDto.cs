using System;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class CertificateTypeDto
	{
		public int Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public DateTime? CreatedAt { get; set; }
		public VerifyStatus Verified { get; set; }
		public string? VerifiedBy { get; set; }
		public DateTime? VerifiedAt { get; set; }

		public ICollection<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();
	}
}
