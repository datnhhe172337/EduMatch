using AutoMapper.Configuration.Annotations;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Text.Json.Serialization;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class TutorEducationDto
	{
		public int Id { get; set; }
		[JsonIgnore]
		public int TutorId { get; set; }
		[JsonIgnore]
		public int InstitutionId { get; set; }
		public DateTime? IssueDate { get; set; }
		public string? CertificateUrl { get; set; }
		[JsonIgnore]
		public string? CertificatePublicId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public VerifyStatus Verified { get; set; }
		public string? RejectReason { get; set; }
		public EducationInstitutionDto? Institution { get; set; }
	}
}
