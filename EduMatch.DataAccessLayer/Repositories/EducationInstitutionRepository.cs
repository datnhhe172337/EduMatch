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

		private IQueryable<EducationInstitution> IncludeAll() =>
			_ctx.EducationInstitutions;

		public async Task<EducationInstitution?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(e => e.Id == id);

		public async Task<EducationInstitution?> GetByCodeAsync(string code)
			=> await IncludeAll().FirstOrDefaultAsync(e => e.Code == code);

		public async Task<IReadOnlyList<EducationInstitution>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		public async Task<IReadOnlyList<EducationInstitution>> GetByNameAsync(string name)
			=> await IncludeAll().Where(e => e.Name.Contains(name)).ToListAsync();

		public async Task<IReadOnlyList<EducationInstitution>> GetByInstitutionTypeAsync(InstitutionType institutionType)
			=> await IncludeAll().Where(e => e.InstitutionType == institutionType).ToListAsync();

		public async Task AddAsync(EducationInstitution entity)
		{
			await _ctx.EducationInstitutions.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task UpdateAsync(EducationInstitution entity)
		{
			_ctx.EducationInstitutions.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.EducationInstitutions.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.EducationInstitutions.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
