using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Subject;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ISubjectService
	{
		/// <summary>
		/// Lấy Subject theo ID
		/// </summary>
		Task<SubjectDto?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy tất cả Subject
		/// </summary>
		Task<IReadOnlyList<SubjectDto>> GetAllAsync();
		/// <summary>
		/// Tìm Subject theo tên
		/// </summary>
		Task<IReadOnlyList<SubjectDto>> GetByNameAsync(string name);
		/// <summary>
		/// Tạo Subject mới
		/// </summary>
		Task<SubjectDto> CreateAsync(SubjectCreateRequest request);
		/// <summary>
		/// Cập nhật Subject
		/// </summary>
		Task<SubjectDto> UpdateAsync(SubjectUpdateRequest request);
		/// <summary>
		/// Xóa Subject theo ID
		/// </summary>
		Task DeleteAsync(int id);
	}
}
