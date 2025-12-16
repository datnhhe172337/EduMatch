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

		private static DateTime GetVietnamNow() =>
			DateTime.UtcNow.AddHours(7);

		public TutorAvailabilityRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorAvailability> IncludeAll() =>
			_ctx.TutorAvailabilities
			.Include(t => t.Slot)
			.Include(t => t.Tutor);

		/// <summary>
		/// Lấy TutorAvailability theo ID với đầy đủ thông tin
		/// </summary>
		public async Task<TutorAvailability?> GetByIdFullAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		/// <summary>
		/// Lấy danh sách TutorAvailability theo TutorId (chỉ lấy những ngày còn tương lai)
		/// </summary>
		public async Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(int tutorId)
		{
			var vietnamNow = GetVietnamNow();
			return await IncludeAll().Where(t => t.TutorId == tutorId && t.StartDate >= vietnamNow).ToListAsync();
		}

		/// <summary>
		/// Lấy danh sách TutorAvailability theo TutorId với đầy đủ thông tin (chỉ lấy những ngày còn tương lai)
		/// </summary>
		public async Task<IReadOnlyList<TutorAvailability>> GetByTutorIdFullAsync(int tutorId)
		{
			var vietnamNow = GetVietnamNow();
			return await IncludeAll().Where(t => t.TutorId == tutorId && t.StartDate >= vietnamNow).ToListAsync();
		}

		/// <summary>
		/// Lấy danh sách TutorAvailability theo trạng thái (chỉ lấy những ngày còn tương lai)
		/// </summary>
		public async Task<IReadOnlyList<TutorAvailability>> GetByStatusAsync(TutorAvailabilityStatus status)
		{
			var vietnamNow = GetVietnamNow();
			return await IncludeAll().Where(t => t.Status == (int)status && t.StartDate >= vietnamNow).ToListAsync();
		}

		/// <summary>
		/// Lấy tất cả TutorAvailability với đầy đủ thông tin (chỉ lấy những ngày còn tương lai)
		/// </summary>
		public async Task<IReadOnlyList<TutorAvailability>> GetAllFullAsync()
		{
			var vietnamNow = GetVietnamNow();
			return await IncludeAll().Where(t => t.StartDate >= vietnamNow).ToListAsync();
		}

		/// <summary>
		/// Thêm TutorAvailability mới
		/// </summary>
		public async Task AddAsync(TutorAvailability entity)
		{
			await _ctx.TutorAvailabilities.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Thêm nhiều TutorAvailability
		/// </summary>
		public async Task AddRangeAsync(IEnumerable<TutorAvailability> entity)
		{
			await _ctx.TutorAvailabilities.AddRangeAsync(entity);
			await _ctx.SaveChangesAsync();
		}
		/// <summary>
		/// Cập nhật TutorAvailability
		/// </summary>
		public async Task UpdateAsync(TutorAvailability entity)
		{
			_ctx.TutorAvailabilities.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Xóa TutorAvailability theo ID
		/// </summary>
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
