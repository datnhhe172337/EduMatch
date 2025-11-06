using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class CertificateTypeSubjectRepository : ICertificateTypeSubjectRepository
	{
		private readonly EduMatchContext _ctx;
		public CertificateTypeSubjectRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<CertificateTypeSubject> IncludeAll() =>
			_ctx.CertificateTypeSubjects;

		/// <summary>
		/// Lấy CertificateTypeSubject theo ID
		/// </summary>
		public async Task<CertificateTypeSubject?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.Id == id);

		/// <summary>
		/// Lấy danh sách CertificateTypeSubject theo CertificateTypeId
		/// </summary>
		public async Task<IReadOnlyList<CertificateTypeSubject>> GetByCertificateTypeIdAsync(int certificateTypeId)
			=> await IncludeAll().Where(c => c.CertificateTypeId == certificateTypeId).ToListAsync();

		/// <summary>
		/// Lấy danh sách CertificateTypeSubject theo SubjectId
		/// </summary>
		public async Task<IReadOnlyList<CertificateTypeSubject>> GetBySubjectIdAsync(int subjectId)
			=> await IncludeAll().Where(c => c.SubjectId == subjectId).ToListAsync();

		/// <summary>
		/// Lấy CertificateTypeSubject theo CertificateTypeId và SubjectId
		/// </summary>
		public async Task<CertificateTypeSubject?> GetByCertificateTypeAndSubjectAsync(int certificateTypeId, int subjectId)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.CertificateTypeId == certificateTypeId && c.SubjectId == subjectId);

		/// <summary>
		/// Lấy tất cả CertificateTypeSubject
		/// </summary>
		public async Task<IReadOnlyList<CertificateTypeSubject>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		/// <summary>
		/// Thêm CertificateTypeSubject mới
		/// </summary>
		public async Task AddAsync(CertificateTypeSubject entity)
		{
			await _ctx.CertificateTypeSubjects.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Cập nhật CertificateTypeSubject
		/// </summary>
		public async Task UpdateAsync(CertificateTypeSubject entity)
		{
			_ctx.CertificateTypeSubjects.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Xóa CertificateTypeSubject theo ID
		/// </summary>
		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.CertificateTypeSubjects.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.CertificateTypeSubjects.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Thêm nhiều CertificateTypeSubject
		/// </summary>
		public async Task AddRangeAsync(IEnumerable<CertificateTypeSubject> entities)
		{
			await _ctx.CertificateTypeSubjects.AddRangeAsync(entities);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Xóa tất cả CertificateTypeSubject theo CertificateTypeId
		/// </summary>
		public async Task RemoveByCertificateTypeIdAsync(int certificateTypeId)
		{
			var entities = await _ctx.CertificateTypeSubjects
				.Where(c => c.CertificateTypeId == certificateTypeId)
				.ToListAsync();
			
			if (entities.Any())
			{
				_ctx.CertificateTypeSubjects.RemoveRange(entities);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
