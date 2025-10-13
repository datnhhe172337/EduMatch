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
			_ctx.TimeSlots.AsNoTracking();

		public async Task<TimeSlot?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id, ct);

		public async Task<IReadOnlyList<TimeSlot>> GetAllAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync(ct);

		public async Task<IReadOnlyList<TimeSlot>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.StartTime >= startTime && t.EndTime <= endTime).ToListAsync(ct);

		public async Task<TimeSlot?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.StartTime == startTime && t.EndTime == endTime, ct);

		public async Task AddAsync(TimeSlot entity, CancellationToken ct = default)
		{
			await _ctx.TimeSlots.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(TimeSlot entity, CancellationToken ct = default)
		{
			_ctx.TimeSlots.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.TimeSlots.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.TimeSlots.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
