using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class CertificateTypeRepository : ICertificateTypeRepository
	{
		private readonly EduMatchContext _ctx;
		public CertificateTypeRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<CertificateType> IncludeAll() =>
			_ctx.CertificateTypes
			.AsNoTracking()
			.AsSplitQuery()
			.Include(c => c.CertificateTypeSubjects)
		 		.ThenInclude(c => c.Subject);

		public async Task<CertificateType?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.Id == id, ct);

		public async Task<CertificateType?> GetByCodeAsync(string code, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.Code == code, ct);

		public async Task<IReadOnlyList<CertificateType>> GetAllAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync(ct);

		public async Task<IReadOnlyList<CertificateType>> GetByNameAsync(string name, CancellationToken ct = default)
			=> await IncludeAll().Where(c => c.Name.Contains(name)).ToListAsync(ct);

		public async Task AddAsync(CertificateType entity, CancellationToken ct = default)
		{
			await _ctx.CertificateTypes.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(CertificateType entity, CancellationToken ct = default)
		{
			_ctx.CertificateTypes.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.CertificateTypes.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.CertificateTypes.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
