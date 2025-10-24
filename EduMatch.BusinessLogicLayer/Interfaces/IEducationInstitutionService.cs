using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.EducationInstitution;
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
		Task<EducationInstitutionDto> CreateAsync(EducationInstitutionCreateRequest request);
		Task<EducationInstitutionDto> UpdateAsync(EducationInstitutionUpdateRequest request);
		Task DeleteAsync(int id);
		Task<EducationInstitutionDto> VerifyAsync(int id, string verifiedBy);
	}
}
