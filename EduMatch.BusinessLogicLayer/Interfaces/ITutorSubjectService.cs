using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITutorSubjectService
	{
		/// <summary>
		/// Lấy TutorSubject theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorSubjectDto?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy TutorSubject theo TutorId với đầy đủ thông tin
		/// </summary>
		Task<TutorSubjectDto?> GetByTutorIdFullAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo TutorId
		/// </summary>
		Task<IReadOnlyList<TutorSubjectDto>> GetByTutorIdAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo SubjectId
		/// </summary>
		Task<IReadOnlyList<TutorSubjectDto>> GetBySubjectIdAsync(int subjectId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo LevelId
		/// </summary>
		Task<IReadOnlyList<TutorSubjectDto>> GetByLevelIdAsync(int levelId);
		/// <summary>
		/// Lấy danh sách TutorSubject theo khoảng giá giờ
		/// </summary>
		Task<IReadOnlyList<TutorSubjectDto>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate);
		/// <summary>
		/// Lấy danh sách TutorSubject theo SubjectId và LevelId
		/// </summary>
		Task<IReadOnlyList<TutorSubjectDto>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId);
		/// <summary>
		/// Lấy tất cả TutorSubject với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorSubjectDto>> GetAllFullAsync();
		/// <summary>
		/// Tạo TutorSubject mới
		/// </summary>
		Task<TutorSubjectDto> CreateAsync(TutorSubjectCreateRequest request);
		/// <summary>
		/// Cập nhật TutorSubject
		/// </summary>
		Task<TutorSubjectDto> UpdateAsync(TutorSubjectUpdateRequest request);
		/// <summary>
		/// Tạo nhiều TutorSubject
		/// </summary>
		Task<List<TutorSubjectDto>> CreateBulkAsync(List<TutorSubjectCreateRequest> requests);
		/// <summary>
		/// Xóa TutorSubject theo ID
		/// </summary>
		Task DeleteAsync(int id);
		/// <summary>
		/// Xóa tất cả TutorSubject theo TutorId
		/// </summary>
		Task DeleteByTutorIdAsync(int tutorId);
    }
}
