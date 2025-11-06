using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ISubjectRepository
	{
		/// <summary>
		/// Lấy Subject theo ID
		/// </summary>
		Task<Subject?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy tất cả Subject
		/// </summary>
		Task<IReadOnlyList<Subject>> GetAllAsync();
		/// <summary>
		/// Tìm Subject theo tên
		/// </summary>
		Task<IReadOnlyList<Subject>> GetByNameAsync(string name);
		/// <summary>
		/// Thêm Subject mới
		/// </summary>
		Task AddAsync(Subject entity);
		/// <summary>
		/// Cập nhật Subject
		/// </summary>
		Task UpdateAsync(Subject entity);
		/// <summary>
		/// Xóa Subject theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
	}
}
