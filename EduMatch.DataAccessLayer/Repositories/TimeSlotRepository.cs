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

		/// <summary>
		/// Lấy TimeSlot theo ID
		/// </summary>
		public async Task<TimeSlot?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		/// <summary>
		/// Lấy tất cả TimeSlot
		/// </summary>
		public async Task<IReadOnlyList<TimeSlot>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		/// <summary>
		/// Lấy TimeSlot theo khoảng thời gian
		/// </summary>
		public async Task<IReadOnlyList<TimeSlot>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime)
			=> await IncludeAll().Where(t => t.StartTime >= startTime && t.EndTime <= endTime).ToListAsync();

		/// <summary>
		/// Lấy TimeSlot theo thời gian chính xác
		/// </summary>
		public async Task<TimeSlot?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.StartTime == startTime && t.EndTime == endTime);

		/// <summary>
		/// Thêm TimeSlot mới
		/// </summary>
		public async Task AddAsync(TimeSlot entity)
		{
			await _ctx.TimeSlots.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Cập nhật TimeSlot
		/// </summary>
		public async Task UpdateAsync(TimeSlot entity)
		{
			_ctx.TimeSlots.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Xóa TimeSlot theo ID
		/// </summary>
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
