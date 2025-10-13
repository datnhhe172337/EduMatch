using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class LevelRepository : ILevelRepository
	{
		private readonly EduMatchContext _ctx;
		public LevelRepository(EduMatchContext ctx) => _ctx = ctx;

		public async Task<Level?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await _ctx.Levels.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);

		public async Task<IReadOnlyList<Level>> GetAllAsync(CancellationToken ct = default)
			=> await _ctx.Levels.AsNoTracking().ToListAsync(ct);

		public async Task<IReadOnlyList<Level>> GetByNameAsync(string name, CancellationToken ct = default)
			=> await _ctx.Levels.AsNoTracking().Where(l => l.Name.Contains(name)).ToListAsync(ct);

		public async Task AddAsync(Level entity, CancellationToken ct = default)
		{
			await _ctx.Levels.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(Level entity, CancellationToken ct = default)
		{
			_ctx.Levels.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.Levels.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.Levels.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
