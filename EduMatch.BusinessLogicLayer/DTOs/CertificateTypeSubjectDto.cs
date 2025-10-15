namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class CertificateTypeSubjectDto
	{
		public int Id { get; set; }
		public int CertificateTypeId { get; set; }
		public int SubjectId { get; set; }
		public CertificateTypeDto? CertificateType { get; set; }
		public SubjectDto? Subject { get; set; }
	}
}
