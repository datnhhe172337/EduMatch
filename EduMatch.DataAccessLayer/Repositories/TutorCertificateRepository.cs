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
			.AsNoTracking()
			.Include(t => t.CertificateType)
			.Include(t => t.Tutor);

		public async Task<TutorCertificate?> GetByIdFullAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id, ct);

		public async Task<TutorCertificate?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.TutorId == tutorId, ct);

	public async Task<IReadOnlyList<TutorCertificate>> GetByTutorIdAsync(int tutorId)
		=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync();

		public async Task<IReadOnlyList<TutorCertificate>> GetByCertificateTypeAsync(int certificateTypeId, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.CertificateTypeId == certificateTypeId).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorCertificate>> GetByVerifiedStatusAsync(VerifyStatus verified, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.Verified == verified).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorCertificate>> GetExpiredCertificatesAsync(CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value < DateTime.Now).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorCertificate>> GetExpiringCertificatesAsync(DateTime beforeDate, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value <= beforeDate).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorCertificate>> GetAllFullAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync(ct);

		public async Task AddAsync(TutorCertificate entity, CancellationToken ct = default)
		{
			await _ctx.TutorCertificates.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(TutorCertificate entity, CancellationToken ct = default)
		{
			_ctx.TutorCertificates.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.TutorCertificates.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.TutorCertificates.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}

		public async Task RemoveByTutorIdAsync(int tutorId, CancellationToken ct = default)
		{
			var entities = await _ctx.TutorCertificates.Where(t => t.TutorId == tutorId).ToListAsync(ct);
			if (entities.Any())
			{
				_ctx.TutorCertificates.RemoveRange(entities);
				await _ctx.SaveChangesAsync(ct);
			}
		}

		
	}
}
