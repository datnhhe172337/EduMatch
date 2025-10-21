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

		public async Task<CertificateTypeSubject?> GetByIdAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.Id == id);

		public async Task<IReadOnlyList<CertificateTypeSubject>> GetByCertificateTypeIdAsync(int certificateTypeId)
			=> await IncludeAll().Where(c => c.CertificateTypeId == certificateTypeId).ToListAsync();

		public async Task<IReadOnlyList<CertificateTypeSubject>> GetBySubjectIdAsync(int subjectId)
			=> await IncludeAll().Where(c => c.SubjectId == subjectId).ToListAsync();

		public async Task<CertificateTypeSubject?> GetByCertificateTypeAndSubjectAsync(int certificateTypeId, int subjectId)
			=> await IncludeAll().FirstOrDefaultAsync(c => c.CertificateTypeId == certificateTypeId && c.SubjectId == subjectId);

		public async Task<IReadOnlyList<CertificateTypeSubject>> GetAllAsync()
			=> await IncludeAll().ToListAsync();

		public async Task AddAsync(CertificateTypeSubject entity)
		{
			await _ctx.CertificateTypeSubjects.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task UpdateAsync(CertificateTypeSubject entity)
		{
			_ctx.CertificateTypeSubjects.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.CertificateTypeSubjects.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.CertificateTypeSubjects.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}
	}
}
