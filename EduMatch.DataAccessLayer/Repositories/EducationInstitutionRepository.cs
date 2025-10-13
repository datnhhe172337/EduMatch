using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class EducationInstitutionRepository : IEducationInstitutionRepository
	{
		private readonly EduMatchContext _ctx;
		public EducationInstitutionRepository(EduMatchContext ctx) => _ctx = ctx;

		public async Task<EducationInstitution?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await _ctx.EducationInstitutions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

		public async Task<EducationInstitution?> GetByCodeAsync(string code, CancellationToken ct = default)
			=> await _ctx.EducationInstitutions.AsNoTracking().FirstOrDefaultAsync(e => e.Code == code, ct);

		public async Task<IReadOnlyList<EducationInstitution>> GetAllAsync(CancellationToken ct = default)
			=> await _ctx.EducationInstitutions.AsNoTracking().ToListAsync(ct);

		public async Task<IReadOnlyList<EducationInstitution>> GetByNameAsync(string name, CancellationToken ct = default)
			=> await _ctx.EducationInstitutions.AsNoTracking().Where(e => e.Name.Contains(name)).ToListAsync(ct);

		public async Task<IReadOnlyList<EducationInstitution>> GetByInstitutionTypeAsync(InstitutionType institutionType, CancellationToken ct = default)
			=> await _ctx.EducationInstitutions.AsNoTracking().Where(e => e.InstitutionType == institutionType).ToListAsync(ct);

		public async Task AddAsync(EducationInstitution entity, CancellationToken ct = default)
		{
			await _ctx.EducationInstitutions.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(EducationInstitution entity, CancellationToken ct = default)
		{
			_ctx.EducationInstitutions.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.EducationInstitutions.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.EducationInstitutions.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
