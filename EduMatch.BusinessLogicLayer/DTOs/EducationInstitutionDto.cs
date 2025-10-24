using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class EducationInstitutionDto
	{
		public int Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public InstitutionType? InstitutionType { get; set; }
		public DateTime? CreatedAt { get; set; }
		public VerifyStatus Verified { get; set; }
		public string? VerifiedBy { get; set; }
		public DateTime? VerifiedAt { get; set; }
	}
}
