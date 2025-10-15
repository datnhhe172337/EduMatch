using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface IEducationInstitutionRepository
	{
		Task<EducationInstitution?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<EducationInstitution?> GetByCodeAsync(string code, CancellationToken ct = default);
		Task<IReadOnlyList<EducationInstitution>> GetAllAsync(CancellationToken ct = default);
		Task<IReadOnlyList<EducationInstitution>> GetByNameAsync(string name, CancellationToken ct = default);
		Task<IReadOnlyList<EducationInstitution>> GetByInstitutionTypeAsync(InstitutionType institutionType, CancellationToken ct = default);
		Task AddAsync(EducationInstitution entity, CancellationToken ct = default);
		Task UpdateAsync(EducationInstitution entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
