namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class SubjectDto
	{
		public int Id { get; set; }
		public string SubjectName { get; set; } = null!;

		public ICollection <CertificateTypeDto>? CertificateTypes { get; set; } = new List<CertificateTypeDto>();
	}
}
