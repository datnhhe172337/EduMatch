using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ILevelService
	{
		Task<LevelDto?> GetByIdAsync(int id);
		Task<IReadOnlyList<LevelDto>> GetAllAsync();
		Task<IReadOnlyList<LevelDto>> GetByNameAsync(string name);
		Task<LevelDto> CreateAsync(LevelCreateRequest request);
		Task<LevelDto> UpdateAsync(LevelUpdateRequest request);
		Task DeleteAsync(int id);
	}
}
