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
			.AsSplitQuery()
			.Include(c => c.CertificateTypeSubjects)
					.ThenInclude(c => c.Subject);

		public async Task<CertificateType?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.Id == id);

		public async Task<CertificateType?> GetByCodeAsync(string code)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.Code == code);

		public async Task<IReadOnlyList<CertificateType>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		public async Task<IReadOnlyList<CertificateType>> GetByNameAsync(string name)
			=> await IncludeAll().Where(c => c.Name.Contains(name)).ToListAsync();

		public async Task AddAsync(CertificateType entity)
		{
			await _ctx.CertificateTypes.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task UpdateAsync(CertificateType entity)
		{
			_ctx.CertificateTypes.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.CertificateTypes
				.Include(c => c.CertificateTypeSubjects)
				.FirstOrDefaultAsync(c => c.Id == id);
			
			if (entity != null)
			{
				// Remove all related CertificateTypeSubjects first
				_ctx.CertificateTypeSubjects.RemoveRange(entity.CertificateTypeSubjects);
				
				// Then remove the CertificateType
				_ctx.CertificateTypes.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
