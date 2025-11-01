using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Subject;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ISubjectService
	{
		Task<SubjectDto?> GetByIdAsync(int id);
		Task<IReadOnlyList<SubjectDto>> GetAllAsync();
		Task<IReadOnlyList<SubjectDto>> GetByNameAsync(string name);
		Task<SubjectDto> CreateAsync(SubjectCreateRequest request);
		Task<SubjectDto> UpdateAsync(SubjectUpdateRequest request);
		Task DeleteAsync(int id);
	}
}
