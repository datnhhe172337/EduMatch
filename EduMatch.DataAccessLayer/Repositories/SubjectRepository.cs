using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class SubjectRepository : ISubjectRepository
	{
		private readonly EduMatchContext _ctx;
		public SubjectRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<Subject> IncludeAll() =>
			_ctx.Subjects
			.Include(s => s.CertificateTypeSubjects)
				.ThenInclude(s => s.CertificateType);

		/// <summary>
		/// Lấy Subject theo ID
		/// </summary>
		public async Task<Subject?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(s => s.Id == id);

		/// <summary>
		/// Lấy tất cả Subject
		/// </summary>
		public async Task<IReadOnlyList<Subject>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		/// <summary>
		/// Tìm Subject theo tên
		/// </summary>
		public async Task<IReadOnlyList<Subject>> GetByNameAsync(string name)
			=> await IncludeAll().Where(s => s.SubjectName.Contains(name)).ToListAsync();

		/// <summary>
		/// Thêm Subject mới
		/// </summary>
		public async Task AddAsync(Subject entity)
		{
			await _ctx.Subjects.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Cập nhật Subject
		/// </summary>
		public async Task UpdateAsync(Subject entity)
		{
			_ctx.Subjects.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Xóa Subject theo ID
		/// </summary>
		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.Subjects.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.Subjects.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
