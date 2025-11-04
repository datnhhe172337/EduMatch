using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.EducationInstitution;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface IEducationInstitutionService
	{
		/// <summary>
		/// Lấy EducationInstitution theo ID
		/// </summary>
		Task<EducationInstitutionDto?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy EducationInstitution theo Code
		/// </summary>
		Task<EducationInstitutionDto?> GetByCodeAsync(string code);
		/// <summary>
		/// Lấy tất cả EducationInstitution
		/// </summary>
		Task<IReadOnlyList<EducationInstitutionDto>> GetAllAsync();
		/// <summary>
		/// Tìm EducationInstitution theo tên
		/// </summary>
		Task<IReadOnlyList<EducationInstitutionDto>> GetByNameAsync(string name);
		/// <summary>
		/// Lấy EducationInstitution theo loại trường
		/// </summary>
		Task<IReadOnlyList<EducationInstitutionDto>> GetByInstitutionTypeAsync(InstitutionType institutionType);
		/// <summary>
		/// Tạo EducationInstitution mới
		/// </summary>
		Task<EducationInstitutionDto> CreateAsync(EducationInstitutionCreateRequest request);
		/// <summary>
		/// Cập nhật EducationInstitution
		/// </summary>
		Task<EducationInstitutionDto> UpdateAsync(EducationInstitutionUpdateRequest request);
		/// <summary>
		/// Xóa EducationInstitution theo ID
		/// </summary>
		Task DeleteAsync(int id);
		/// <summary>
		/// Xác thực EducationInstitution
		/// </summary>
		Task<EducationInstitutionDto> VerifyAsync(int id, string verifiedBy);
	}
}
