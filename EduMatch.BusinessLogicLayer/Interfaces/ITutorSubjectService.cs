using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITutorSubjectService
	{
		Task<TutorSubjectDto?> GetByIdFullAsync(int id);
		Task<TutorSubjectDto?> GetByTutorIdFullAsync(int tutorId);
		Task<IReadOnlyList<TutorSubjectDto>> GetByTutorIdAsync(int tutorId);
		Task<IReadOnlyList<TutorSubjectDto>> GetBySubjectIdAsync(int subjectId);
		Task<IReadOnlyList<TutorSubjectDto>> GetByLevelIdAsync(int levelId);
		Task<IReadOnlyList<TutorSubjectDto>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate);
		Task<IReadOnlyList<TutorSubjectDto>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId);
		Task<IReadOnlyList<TutorSubjectDto>> GetAllFullAsync();
		Task<TutorSubjectDto> CreateAsync(TutorSubjectCreateRequest request);
		Task<TutorSubjectDto> UpdateAsync(TutorSubjectUpdateRequest request);
		Task<List<TutorSubjectDto>> CreateBulkAsync(List<TutorSubjectCreateRequest> requests);
		Task DeleteAsync(int id);
		Task DeleteByTutorIdAsync(int tutorId);
	}
}
