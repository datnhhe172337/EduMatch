using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ISubjectRepository
	{
		Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken ct = default);
		Task<IReadOnlyList<Subject>> GetByNameAsync(string name, CancellationToken ct = default);
		Task AddAsync(Subject entity, CancellationToken ct = default);
		Task UpdateAsync(Subject entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
