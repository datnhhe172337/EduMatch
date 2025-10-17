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
		Task<TutorProfile?> GetByIdFullAsync(int id, CancellationToken ct = default);
		Task<TutorProfile?> GetByEmailFullAsync(string email, CancellationToken ct = default);
		Task<IReadOnlyList<TutorProfile>> GetAllFullAsync(CancellationToken ct = default);

		Task AddAsync(TutorProfile entity, CancellationToken ct = default);
		Task UpdateAsync(TutorProfile entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);



	}
}
