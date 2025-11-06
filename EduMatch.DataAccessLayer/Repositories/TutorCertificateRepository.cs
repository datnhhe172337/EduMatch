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
	public sealed class TutorCertificateRepository : ITutorCertificateRepository
	{
		private readonly EduMatchContext _ctx;
		public TutorCertificateRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorCertificate> IncludeAll() =>
			_ctx.TutorCertificates
			.Include(t => t.CertificateType)
			.Include(t => t.Tutor);

		/// <summary>
		/// Lấy TutorCertificate theo ID với đầy đủ thông tin
		/// </summary>
		public async Task<TutorCertificate?> GetByIdFullAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		/// <summary>
		/// Lấy TutorCertificate theo TutorId với đầy đủ thông tin
		/// </summary>
		public async Task<TutorCertificate?> GetByTutorIdFullAsync(int tutorId)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.TutorId == tutorId);

		/// <summary>
		/// Lấy danh sách TutorCertificate theo TutorId
		/// </summary>
	public async Task<IReadOnlyList<TutorCertificate>> GetByTutorIdAsync(int tutorId)
		=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync();

		/// <summary>
		/// Lấy danh sách TutorCertificate theo CertificateTypeId
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificate>> GetByCertificateTypeAsync(int certificateTypeId)
			=> await IncludeAll().Where(t => t.CertificateTypeId == certificateTypeId).ToListAsync();

		/// <summary>
		/// Lấy danh sách TutorCertificate theo trạng thái xác thực
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificate>> GetByVerifiedStatusAsync(VerifyStatus verified)
			=> await IncludeAll().Where(t => t.Verified == (int)verified).ToListAsync();

		/// <summary>
		/// Lấy danh sách TutorCertificate đã hết hạn
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificate>> GetExpiredCertificatesAsync()
			=> await IncludeAll().Where(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value < DateTime.Now).ToListAsync();

		/// <summary>
		/// Lấy danh sách TutorCertificate sắp hết hạn
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificate>> GetExpiringCertificatesAsync(DateTime beforeDate)
			=> await IncludeAll().Where(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value <= beforeDate).ToListAsync();

		/// <summary>
		/// Lấy tất cả TutorCertificate với đầy đủ thông tin
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificate>> GetAllFullAsync()
			=> await IncludeAll().ToListAsync();

		/// <summary>
		/// Thêm TutorCertificate mới
		/// </summary>
		public async Task AddAsync(TutorCertificate entity)
		{
			await _ctx.TutorCertificates.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Cập nhật TutorCertificate
		/// </summary>
		public async Task UpdateAsync(TutorCertificate entity)
		{
			_ctx.TutorCertificates.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		/// <summary>
		/// Xóa TutorCertificate theo ID
		/// </summary>
		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.TutorCertificates.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.TutorCertificates.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Xóa tất cả TutorCertificate theo TutorId
		/// </summary>
		public async Task RemoveByTutorIdAsync(int tutorId)
		{
			var entities = await _ctx.TutorCertificates.Where(t => t.TutorId == tutorId).ToListAsync();
			if (entities.Any())
			{
				_ctx.TutorCertificates.RemoveRange(entities);
				await _ctx.SaveChangesAsync();
			}
		}

		
	}
}
