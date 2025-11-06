using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorEducationRepository
	{
		/// <summary>
		/// Lấy TutorEducation theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorEducation?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy TutorEducation theo TutorId với đầy đủ thông tin
		/// </summary>
		Task<TutorEducation?> GetByTutorIdFullAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorEducation theo TutorId
		/// </summary>
		Task<IReadOnlyList<TutorEducation>> GetByTutorIdAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorEducation theo InstitutionId
		/// </summary>
		Task<IReadOnlyList<TutorEducation>> GetByInstitutionIdAsync(int institutionId);
		/// <summary>
		/// Lấy danh sách TutorEducation theo trạng thái xác thực
		/// </summary>
		Task<IReadOnlyList<TutorEducation>> GetByVerifiedStatusAsync(VerifyStatus verified);
		/// <summary>
		/// Lấy danh sách TutorEducation đang chờ xác thực
		/// </summary>
		Task<IReadOnlyList<TutorEducation>> GetPendingVerificationsAsync();
		/// <summary>
		/// Lấy tất cả TutorEducation với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorEducation>> GetAllFullAsync();
		/// <summary>
		/// Thêm TutorEducation mới
		/// </summary>
		Task AddAsync(TutorEducation entity);
		/// <summary>
		/// Cập nhật TutorEducation
		/// </summary>
		Task UpdateAsync(TutorEducation entity);
		/// <summary>
		/// Xóa TutorEducation theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
		/// <summary>
		/// Xóa tất cả TutorEducation theo TutorId
		/// </summary>
		Task RemoveByTutorIdAsync(int tutorId);
	}
}
