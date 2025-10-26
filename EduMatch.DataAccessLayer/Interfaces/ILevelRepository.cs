using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ILevelRepository
	{
		Task<Level?> GetByIdAsync(int id);
		Task<IReadOnlyList<Level>> GetAllAsync();
		Task<IReadOnlyList<Level>> GetByNameAsync(string name);
		Task AddAsync(Level entity);
		Task UpdateAsync(Level entity);
		Task RemoveByIdAsync(int id);
	}
}
