using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class TutorEducationRepository : ITutorEducationRepository
	{
		private readonly EduMatchContext _ctx;
		public TutorEducationRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorEducation> IncludeAll() =>
			_ctx.TutorEducations
			.AsNoTracking()
			.Include(t => t.Institution)
			.Include(t => t.Tutor);

		public async Task<TutorEducation?> GetByIdFullAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id, ct);

		public async Task<TutorEducation?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.TutorId == tutorId, ct);

	public async Task<IReadOnlyList<TutorEducation>> GetByTutorIdAsync(int tutorId)
		=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync();

		public async Task<IReadOnlyList<TutorEducation>> GetByInstitutionIdAsync(int institutionId, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.InstitutionId == institutionId).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorEducation>> GetByVerifiedStatusAsync(VerifyStatus verified, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.Verified == verified).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorEducation>> GetPendingVerificationsAsync(CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.Verified == VerifyStatus.Pending).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorEducation>> GetAllFullAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync(ct);

		public async Task AddAsync(TutorEducation entity, CancellationToken ct = default)
		{
			await _ctx.TutorEducations.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(TutorEducation entity, CancellationToken ct = default)
		{
			_ctx.TutorEducations.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.TutorEducations.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.TutorEducations.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}

		public async Task RemoveByTutorIdAsync(int tutorId, CancellationToken ct = default)
		{
			var entities = await _ctx.TutorEducations.Where(t => t.TutorId == tutorId).ToListAsync(ct);
			if (entities.Any())
			{
				_ctx.TutorEducations.RemoveRange(entities);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
