using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class EducationInstitutionLevelRepository : IEducationInstitutionLevelRepository
	{
		private readonly EduMatchContext _ctx;
		public EducationInstitutionLevelRepository(EduMatchContext ctx) => _ctx = ctx;

		public async Task<EducationInstitutionLevel?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await _ctx.EducationInstitutionLevels.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

		public async Task<IReadOnlyList<EducationInstitutionLevel>> GetByInstitutionIdAsync(int institutionId, CancellationToken ct = default)
			=> await _ctx.EducationInstitutionLevels.AsNoTracking().Where(e => e.InstitutionId == institutionId).ToListAsync(ct);

		public async Task<IReadOnlyList<EducationInstitutionLevel>> GetByEducationLevelIdAsync(int educationLevelId, CancellationToken ct = default)
			=> await _ctx.EducationInstitutionLevels.AsNoTracking().Where(e => e.EducationLevelId == educationLevelId).ToListAsync(ct);

		public async Task<EducationInstitutionLevel?> GetByInstitutionAndLevelAsync(int institutionId, int educationLevelId, CancellationToken ct = default)
			=> await _ctx.EducationInstitutionLevels.AsNoTracking().FirstOrDefaultAsync(e => e.InstitutionId == institutionId && e.EducationLevelId == educationLevelId, ct);

		public async Task<IReadOnlyList<EducationInstitutionLevel>> GetAllAsync(CancellationToken ct = default)
			=> await _ctx.EducationInstitutionLevels.AsNoTracking().ToListAsync(ct);

		public async Task AddAsync(EducationInstitutionLevel entity, CancellationToken ct = default)
		{
			await _ctx.EducationInstitutionLevels.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(EducationInstitutionLevel entity, CancellationToken ct = default)
		{
			_ctx.EducationInstitutionLevels.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.EducationInstitutionLevels.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.EducationInstitutionLevels.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
