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

		public async Task<TutorCertificate?> GetByIdFullAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		public async Task<TutorCertificate?> GetByTutorIdFullAsync(int tutorId)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.TutorId == tutorId);

	public async Task<IReadOnlyList<TutorCertificate>> GetByTutorIdAsync(int tutorId)
		=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync();

		public async Task<IReadOnlyList<TutorCertificate>> GetByCertificateTypeAsync(int certificateTypeId)
			=> await IncludeAll().Where(t => t.CertificateTypeId == certificateTypeId).ToListAsync();

		public async Task<IReadOnlyList<TutorCertificate>> GetByVerifiedStatusAsync(VerifyStatus verified)
			=> await IncludeAll().Where(t => t.Verified == verified).ToListAsync();

		public async Task<IReadOnlyList<TutorCertificate>> GetExpiredCertificatesAsync()
			=> await IncludeAll().Where(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value < DateTime.Now).ToListAsync();

		public async Task<IReadOnlyList<TutorCertificate>> GetExpiringCertificatesAsync(DateTime beforeDate)
			=> await IncludeAll().Where(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value <= beforeDate).ToListAsync();

		public async Task<IReadOnlyList<TutorCertificate>> GetAllFullAsync()
			=> await IncludeAll().ToListAsync();

		public async Task AddAsync(TutorCertificate entity)
		{
			await _ctx.TutorCertificates.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task UpdateAsync(TutorCertificate entity)
		{
			_ctx.TutorCertificates.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.TutorCertificates.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.TutorCertificates.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}

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
