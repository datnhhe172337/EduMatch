using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class TutorProfileRepository : ITutorProfileRepository
	{
		private readonly EduMatchContext _ctx;
		public TutorProfileRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorProfile> IncludeAll() =>
			_ctx.TutorProfiles
			.AsNoTracking()
			.AsSplitQuery()
			.Include(t => t.TutorAvailabilities)
			   .ThenInclude(t => t.Slot)
			.Include(t => t.TutorCertificates)
			.Include(t => t.TutorEducations)
			.Include(t => t.TutorSubjects)
			.Include(t => t.UserEmailNavigation)
				.ThenInclude(t => t.UserProfile);


		public async Task<TutorProfile?> GetByIdFullAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id, ct);

		public async Task<TutorProfile?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == tutorId, ct);

		public async Task<TutorProfile?> GetByEmailFullAsync(string email, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.UserEmail == email, ct);

		public async Task<IReadOnlyList<TutorProfile>> GetAllFullAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync();

		public async Task AddAsync(TutorProfile entity, CancellationToken ct = default)
		{
			await _ctx.TutorProfiles.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(TutorProfile entity, CancellationToken ct = default)
		{
			_ctx.TutorProfiles.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.TutorProfiles.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.TutorProfiles.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
