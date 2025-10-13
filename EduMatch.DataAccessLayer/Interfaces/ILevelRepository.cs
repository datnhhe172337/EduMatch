using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ILevelRepository
	{
		Task<Level?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<IReadOnlyList<Level>> GetAllAsync(CancellationToken ct = default);
		Task<IReadOnlyList<Level>> GetByNameAsync(string name, CancellationToken ct = default);
		Task AddAsync(Level entity, CancellationToken ct = default);
		Task UpdateAsync(Level entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
