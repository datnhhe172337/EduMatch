using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
	public sealed class TutorSubjectRepository : ITutorSubjectRepository
	{
		private readonly EduMatchContext _ctx;
		public TutorSubjectRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorSubject> IncludeAll() =>
			_ctx.TutorSubjects
			.AsNoTracking()
			.AsSplitQuery()
			.Include(t => t.Level)
			.Include(t => t.Subject)
			.Include(t => t.Tutor);

		public async Task<TutorSubject?> GetByIdFullAsync(int id, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id, ct);

		public async Task<TutorSubject?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.TutorId == tutorId, ct);

		public async Task<IReadOnlyList<TutorSubject>> GetByTutorIdAsync(int tutorId, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorSubject>> GetBySubjectIdAsync(int subjectId, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.SubjectId == subjectId).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorSubject>> GetByLevelIdAsync(int levelId, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.LevelId == levelId).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorSubject>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.HourlyRate.HasValue && t.HourlyRate.Value >= minRate && t.HourlyRate.Value <= maxRate).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorSubject>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId, CancellationToken ct = default)
			=> await IncludeAll().Where(t => t.SubjectId == subjectId && t.LevelId == levelId).ToListAsync(ct);

		public async Task<IReadOnlyList<TutorSubject>> GetAllFullAsync(CancellationToken ct = default)
			=> await IncludeAll().ToListAsync(ct);

		public async Task AddAsync(TutorSubject entity, CancellationToken ct = default)
		{
			await _ctx.TutorSubjects.AddAsync(entity, ct);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(TutorSubject entity, CancellationToken ct = default)
		{
			_ctx.TutorSubjects.Update(entity);
			await _ctx.SaveChangesAsync(ct);
		}

		public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
		{
			var entity = await _ctx.TutorSubjects.FindAsync(new object?[] { id }, ct);
			if (entity != null)
			{
				_ctx.TutorSubjects.Remove(entity);
				await _ctx.SaveChangesAsync(ct);
			}
		}

		public async Task RemoveByTutorIdAsync(int tutorId, CancellationToken ct = default)
		{
			var entities = await _ctx.TutorSubjects.Where(t => t.TutorId == tutorId).ToListAsync(ct);
			if (entities.Any())
			{
				_ctx.TutorSubjects.RemoveRange(entities);
				await _ctx.SaveChangesAsync(ct);
			}
		}
	}
}
