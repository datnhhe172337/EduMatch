using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface IEducationLevelRepository
	{
		Task<EducationLevel?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<EducationLevel?> GetByCodeAsync(string code, CancellationToken ct = default);
		Task<IReadOnlyList<EducationLevel>> GetAllAsync(CancellationToken ct = default);
		Task<IReadOnlyList<EducationLevel>> GetByNameAsync(string name, CancellationToken ct = default);
		Task AddAsync(EducationLevel entity, CancellationToken ct = default);
		Task UpdateAsync(EducationLevel entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
