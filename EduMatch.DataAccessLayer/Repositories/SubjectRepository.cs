using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class SubjectRepository : ISubjectRepository
	{
		private readonly EduMatchContext _ctx;
		public SubjectRepository(EduMatchContext ctx) => _ctx = ctx;

		public async Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await _ctx.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);

		public async Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken ct = default)
			=> await _ctx.Subjects.AsNoTracking().ToListAsync(ct);

		public async Task<IReadOnlyList<Subject>> GetByNameAsync(string name, CancellationToken ct = default)
			=> await _ctx.Subjects.AsNoTracking().Where(s => s.SubjectName.Contains(name)).ToListAsync(ct);

		public async Task AddAsync(Subject entity, CancellationToken ct = default)
		{
			await _ctx.Subjects.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(Subject entity, CancellationToken ct = default)
		{
			_ctx.Subjects.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.Subjects.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.Subjects.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
