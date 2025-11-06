using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface IEducationInstitutionRepository
	{
		/// <summary>
		/// Lấy EducationInstitution theo ID
		/// </summary>
		Task<EducationInstitution?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy EducationInstitution theo Code
		/// </summary>
		Task<EducationInstitution?> GetByCodeAsync(string code);
		/// <summary>
		/// Lấy tất cả EducationInstitution
		/// </summary>
		Task<IReadOnlyList<EducationInstitution>> GetAllAsync();
		/// <summary>
		/// Tìm EducationInstitution theo tên
		/// </summary>
		Task<IReadOnlyList<EducationInstitution>> GetByNameAsync(string name);
		/// <summary>
		/// Lấy EducationInstitution theo loại trường
		/// </summary>
		Task<IReadOnlyList<EducationInstitution>> GetByInstitutionTypeAsync(InstitutionType institutionType);
		/// <summary>
		/// Thêm EducationInstitution mới
		/// </summary>
		Task AddAsync(EducationInstitution entity);
		/// <summary>
		/// Cập nhật EducationInstitution
		/// </summary>
		Task UpdateAsync(EducationInstitution entity);
		/// <summary>
		/// Xóa EducationInstitution theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
	}
}
