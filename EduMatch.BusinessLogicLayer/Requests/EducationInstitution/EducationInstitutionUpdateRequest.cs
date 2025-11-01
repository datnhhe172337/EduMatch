using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.EducationInstitution
{
    public class EducationInstitutionUpdateRequest
    {
        [Required(ErrorMessage = "ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ID must be greater than 0")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "InstitutionType is required")]
		[EnumDataType(typeof(InstitutionType), ErrorMessage = "Invalid InstitutionType")]
        public InstitutionType? InstitutionType { get; set; }
    }
}
