using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface IEducationInstitutionService
	{
		Task<EducationInstitutionDto?> GetByIdAsync(int id);
		Task<EducationInstitutionDto?> GetByCodeAsync(string code);
		Task<IReadOnlyList<EducationInstitutionDto>> GetAllAsync();
		Task<IReadOnlyList<EducationInstitutionDto>> GetByNameAsync(string name);
		Task<IReadOnlyList<EducationInstitutionDto>> GetByInstitutionTypeAsync(InstitutionType institutionType);
		Task<EducationInstitutionDto> CreateAsync(string code, string name, InstitutionType? institutionType = null);
		Task<EducationInstitutionDto> UpdateAsync(int id, string code, string name, InstitutionType? institutionType = null);
		Task DeleteAsync(int id);
	}
}
