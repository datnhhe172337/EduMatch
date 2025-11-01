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
			.Include(t => t.Institution)
			.Include(t => t.Tutor);

		public async Task<TutorEducation?> GetByIdFullAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		public async Task<TutorEducation?> GetByTutorIdFullAsync(int tutorId)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.TutorId == tutorId);

	public async Task<IReadOnlyList<TutorEducation>> GetByTutorIdAsync(int tutorId)
		=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync();

		public async Task<IReadOnlyList<TutorEducation>> GetByInstitutionIdAsync(int institutionId)
			=> await IncludeAll().Where(t => t.InstitutionId == institutionId).ToListAsync();

		public async Task<IReadOnlyList<TutorEducation>> GetByVerifiedStatusAsync(VerifyStatus verified)
			=> await IncludeAll().Where(t => t.Verified == (int)verified).ToListAsync();

		public async Task<IReadOnlyList<TutorEducation>> GetPendingVerificationsAsync()
			=> await IncludeAll().Where(t => t.Verified == (int)VerifyStatus.Pending).ToListAsync();

		public async Task<IReadOnlyList<TutorEducation>> GetAllFullAsync()
			=> await IncludeAll().ToListAsync();

		public async Task AddAsync(TutorEducation entity)
		{
			await _ctx.TutorEducations.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task UpdateAsync(TutorEducation entity)
		{
			_ctx.TutorEducations.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.TutorEducations.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.TutorEducations.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}

		public async Task RemoveByTutorIdAsync(int tutorId)
		{
			var entities = await _ctx.TutorEducations.Where(t => t.TutorId == tutorId).ToListAsync();
			if (entities.Any())
			{
				_ctx.TutorEducations.RemoveRange(entities);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
