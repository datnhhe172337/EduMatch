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

		public async Task<CertificateTypeSubject?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await _ctx.CertificateTypeSubjects.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

		public async Task<IReadOnlyList<CertificateTypeSubject>> GetByCertificateTypeIdAsync(int certificateTypeId, CancellationToken ct = default)
			=> await _ctx.CertificateTypeSubjects.AsNoTracking().Where(c => c.CertificateTypeId == certificateTypeId).ToListAsync(ct);

		public async Task<IReadOnlyList<CertificateTypeSubject>> GetBySubjectIdAsync(int subjectId, CancellationToken ct = default)
			=> await _ctx.CertificateTypeSubjects.AsNoTracking().Where(c => c.SubjectId == subjectId).ToListAsync(ct);

		public async Task<CertificateTypeSubject?> GetByCertificateTypeAndSubjectAsync(int certificateTypeId, int subjectId, CancellationToken ct = default)
			=> await _ctx.CertificateTypeSubjects.AsNoTracking().FirstOrDefaultAsync(c => c.CertificateTypeId == certificateTypeId && c.SubjectId == subjectId, ct);

		public async Task<IReadOnlyList<CertificateTypeSubject>> GetAllAsync(CancellationToken ct = default)
			=> await _ctx.CertificateTypeSubjects.AsNoTracking().ToListAsync(ct);

		public async Task AddAsync(CertificateTypeSubject entity, CancellationToken ct = default)
		{
			await _ctx.CertificateTypeSubjects.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(CertificateTypeSubject entity, CancellationToken ct = default)
		{
			_ctx.CertificateTypeSubjects.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.CertificateTypeSubjects.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.CertificateTypeSubjects.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
