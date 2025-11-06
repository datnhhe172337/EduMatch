using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class LevelRepository : ILevelRepository
	{
		private readonly EduMatchContext _ctx;
		public LevelRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<Level> IncludeAll() =>
			_ctx.Levels;

		/// <summary>
		/// Lấy Level theo ID
		/// </summary>
		public async Task<Level?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(l => l.Id == id);

		/// <summary>
		/// Lấy tất cả Level
		/// </summary>
		public async Task<IReadOnlyList<Level>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		/// <summary>
		/// Tìm Level theo tên
		/// </summary>
		public async Task<IReadOnlyList<Level>> GetByNameAsync(string name)
			=> await IncludeAll().Where(l => l.Name.Contains(name)).ToListAsync();

		/// <summary>
		/// Thêm Level mới
		/// </summary>
		public async Task AddAsync(Level entity)
		{
			await _ctx.Levels.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Cập nhật Level
		/// </summary>
		public async Task UpdateAsync(Level entity)
		{
			_ctx.Levels.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Xóa Level theo ID
		/// </summary>
		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.Levels.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.Levels.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
