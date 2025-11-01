using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface IEducationInstitutionRepository
	{
		Task<EducationInstitution?> GetByIdAsync(int id);
		Task<EducationInstitution?> GetByCodeAsync(string code);
		Task<IReadOnlyList<EducationInstitution>> GetAllAsync();
		Task<IReadOnlyList<EducationInstitution>> GetByNameAsync(string name);
		Task<IReadOnlyList<EducationInstitution>> GetByInstitutionTypeAsync(InstitutionType institutionType);
		Task AddAsync(EducationInstitution entity);
		Task UpdateAsync(EducationInstitution entity);
		Task RemoveByIdAsync(int id);
	}
}
