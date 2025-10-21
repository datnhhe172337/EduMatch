using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorProfileRepository
	{
		Task<TutorProfile?> GetByIdFullAsync(int id);
		Task<TutorProfile?> GetByEmailFullAsync(string email);
		Task<IReadOnlyList<TutorProfile>> GetAllFullAsync();

		Task AddAsync(TutorProfile entity);
		Task UpdateAsync(TutorProfile entity);
		Task RemoveByIdAsync(int id);
	}
}
