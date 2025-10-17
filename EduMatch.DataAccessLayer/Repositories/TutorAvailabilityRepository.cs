using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class TutorAvailabilityRepository : ITutorAvailabilityRepository
	{
		private readonly EduMatchContext _ctx;
		public TutorAvailabilityRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorAvailability> IncludeAll() =>
			_ctx.TutorAvailabilities
			.AsNoTracking()
			.Include(t => t.Slot)
			.Include(t => t.Tutor);

		public async Task<TutorAvailability?> GetByIdFullAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id, ct);

		public async Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(int tutorId, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync(ct);


		public async Task<IReadOnlyList<TutorAvailability>> GetByStatusAsync(TutorAvailabilityStatus status, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.Status == status).ToListAsync(ct);


		public async Task<IReadOnlyList<TutorAvailability>> GetAllFullAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync(ct);

		public async Task AddAsync(TutorAvailability entity, CancellationToken ct = default)
		{
			await _ctx.TutorAvailabilities.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task AddRangeAsync(IEnumerable<TutorAvailability> entity, CancellationToken ct = default)
		{
			await _ctx.TutorAvailabilities.AddRangeAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}
		public async Task UpdateAsync(TutorAvailability entity, CancellationToken ct = default)
		{
			_ctx.TutorAvailabilities.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.TutorAvailabilities.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.TutorAvailabilities.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}

	
		
	}
}
