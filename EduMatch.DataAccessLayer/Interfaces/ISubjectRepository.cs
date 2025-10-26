using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ISubjectRepository
	{
		Task<Subject?> GetByIdAsync(int id);
		Task<IReadOnlyList<Subject>> GetAllAsync();
		Task<IReadOnlyList<Subject>> GetByNameAsync(string name);
		Task AddAsync(Subject entity);
		Task UpdateAsync(Subject entity);
		Task RemoveByIdAsync(int id);
	}
}
