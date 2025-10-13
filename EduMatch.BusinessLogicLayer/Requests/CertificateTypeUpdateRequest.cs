namespace EduMatch.BusinessLogicLayer.Requests
{
	public class CertificateTypeUpdateRequest
	{
		public int Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
	}
}
