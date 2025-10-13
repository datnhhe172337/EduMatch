using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class EducationLevelRepository : IEducationLevelRepository
	{
		private readonly EduMatchContext _ctx;
		public EducationLevelRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<EducationLevel> IncludeAll() =>
			_ctx.EducationLevels.AsNoTracking();

		public async Task<EducationLevel?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(e => e.Id == id, ct);

		public async Task<EducationLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(e => e.Code == code, ct);

		public async Task<IReadOnlyList<EducationLevel>> GetAllAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync(ct);

		public async Task<IReadOnlyList<EducationLevel>> GetByNameAsync(string name, CancellationToken ct = default)
			=> await IncludeAll().Where(e => e.Name.Contains(name)).ToListAsync(ct);

		public async Task AddAsync(EducationLevel entity, CancellationToken ct = default)
		{
			await _ctx.EducationLevels.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(EducationLevel entity, CancellationToken ct = default)
		{
			_ctx.EducationLevels.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.EducationLevels.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.EducationLevels.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
