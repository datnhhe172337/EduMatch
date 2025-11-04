using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorSubjectRepository
	{
		/// <summary>
		/// Lấy TutorSubject theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorSubject?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy TutorSubject theo TutorId với đầy đủ thông tin
		/// </summary>
		Task<TutorSubject?> GetByTutorIdFullAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo TutorId
		/// </summary>
		Task<IReadOnlyList<TutorSubject>> GetByTutorIdAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo SubjectId
		/// </summary>
		Task<IReadOnlyList<TutorSubject>> GetBySubjectIdAsync(int subjectId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo LevelId
		/// </summary>
		Task<IReadOnlyList<TutorSubject>> GetByLevelIdAsync(int levelId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo khoảng giá giờ
		/// </summary>
		Task<IReadOnlyList<TutorSubject>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate);
		/// <summary>
		/// Lấy danh sách TutorSubject theo SubjectId và LevelId
		/// </summary>
		Task<IReadOnlyList<TutorSubject>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId);
		/// <summary>
		/// Lấy tất cả TutorSubject với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorSubject>> GetAllFullAsync();
		/// <summary>
		/// Thêm TutorSubject mới
		/// </summary>
		Task AddAsync(TutorSubject entity);
		/// <summary>
		/// Cập nhật TutorSubject
		/// </summary>
		Task UpdateAsync(TutorSubject entity);
		/// <summary>
		/// Xóa TutorSubject theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
		/// <summary>
		/// Xóa tất cả TutorSubject theo TutorId
		/// </summary>
		Task RemoveByTutorIdAsync(int tutorId);
	}
}
