using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class TutorAvailabilityRepository : ITutorAvailabilityRepository
	{
		private readonly EduMatchContext _ctx;
		public TutorAvailabilityRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorAvailability> IncludeAll() =>
			_ctx.TutorAvailabilities
			.Include(t => t.Slot)
			.Include(t => t.Tutor);

		public async Task<TutorAvailability?> GetByIdFullAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		public async Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(int tutorId)
			=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync();


		public async Task<IReadOnlyList<TutorAvailability>> GetByStatusAsync(TutorAvailabilityStatus status)
			=> await IncludeAll().Where(t => t.Status == status).ToListAsync();


		public async Task<IReadOnlyList<TutorAvailability>> GetAllFullAsync()
			=> await IncludeAll().ToListAsync();

		public async Task AddAsync(TutorAvailability entity)
		{
			await _ctx.TutorAvailabilities.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task AddRangeAsync(IEnumerable<TutorAvailability> entity)
		{
			await _ctx.TutorAvailabilities.AddRangeAsync(entity);
			await _ctx.SaveChangesAsync();
		}
		public async Task UpdateAsync(TutorAvailability entity)
		{
			_ctx.TutorAvailabilities.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.TutorAvailabilities.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.TutorAvailabilities.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}

	
		
	}
}
