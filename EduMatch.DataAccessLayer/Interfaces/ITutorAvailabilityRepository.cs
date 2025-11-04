using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorAvailabilityRepository
	{
		/// <summary>
		/// Lấy TutorAvailability theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorAvailability?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy danh sách TutorAvailability theo TutorId
		/// </summary>
		Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorAvailability theo TutorId với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorAvailability>> GetByTutorIdFullAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorAvailability theo trạng thái
		/// </summary>
		Task<IReadOnlyList<TutorAvailability>> GetByStatusAsync(TutorAvailabilityStatus status);
		/// <summary>
		/// Lấy tất cả TutorAvailability với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorAvailability>> GetAllFullAsync();
		/// <summary>
		/// Thêm TutorAvailability mới
		/// </summary>
		Task AddAsync(TutorAvailability entity);
		/// <summary>
		/// Thêm nhiều TutorAvailability
		/// </summary>
		Task AddRangeAsync(IEnumerable<TutorAvailability> entity);
		/// <summary>
		/// Cập nhật TutorAvailability
		/// </summary>
		Task UpdateAsync(TutorAvailability entity);
		/// <summary>
		/// Xóa TutorAvailability theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
	}
}
