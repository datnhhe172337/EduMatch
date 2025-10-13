using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface IEducationInstitutionLevelRepository
	{
		Task<EducationInstitutionLevel?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<IReadOnlyList<EducationInstitutionLevel>> GetByInstitutionIdAsync(int institutionId, CancellationToken ct = default);
		Task<IReadOnlyList<EducationInstitutionLevel>> GetByEducationLevelIdAsync(int educationLevelId, CancellationToken ct = default);
		Task<EducationInstitutionLevel?> GetByInstitutionAndLevelAsync(int institutionId, int educationLevelId, CancellationToken ct = default);
		Task<IReadOnlyList<EducationInstitutionLevel>> GetAllAsync(CancellationToken ct = default);
		Task AddAsync(EducationInstitutionLevel entity, CancellationToken ct = default);
		Task UpdateAsync(EducationInstitutionLevel entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
