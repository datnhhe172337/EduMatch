using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.CertificateType
{
	public class CertificateTypeUpdateRequest
	{
		[Required(ErrorMessage = "Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
		public int Id { get; set; }

		[Required(ErrorMessage = "Code is required")]
		[StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
		public string Code { get; set; } = null!;

		[Required(ErrorMessage = "Name is required")]
		[StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
		public string Name { get; set; } = null!;
	}
}
