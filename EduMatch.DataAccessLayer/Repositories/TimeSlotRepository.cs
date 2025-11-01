using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class TimeSlotRepository : ITimeSlotRepository
	{
		private readonly EduMatchContext _ctx;
		public TimeSlotRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TimeSlot> IncludeAll() =>
			_ctx.TimeSlots;

		public async Task<TimeSlot?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		public async Task<IReadOnlyList<TimeSlot>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		public async Task<IReadOnlyList<TimeSlot>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime)
			=> await IncludeAll().Where(t => t.StartTime >= startTime && t.EndTime <= endTime).ToListAsync();

		public async Task<TimeSlot?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.StartTime == startTime && t.EndTime == endTime);

		public async Task AddAsync(TimeSlot entity)
		{
			await _ctx.TimeSlots.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task UpdateAsync(TimeSlot entity)
		{
			_ctx.TimeSlots.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.TimeSlots.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.TimeSlots.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
