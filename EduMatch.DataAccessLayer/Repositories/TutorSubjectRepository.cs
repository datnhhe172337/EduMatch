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
			.Include(t => t.Level)
			.Include(t => t.Subject)
			.Include(t => t.Tutor);

	/// <summary>
	/// Lấy TutorSubject theo ID với đầy đủ thông tin
	/// </summary>
	public async Task<TutorSubject?> GetByIdFullAsync(int id)
		=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

	/// <summary>
	/// Lấy TutorSubject theo TutorId với đầy đủ thông tin
	/// </summary>
	public async Task<TutorSubject?> GetByTutorIdFullAsync(int tutorId)
		=> await IncludeAll().FirstOrDefaultAsync(t => t.TutorId == tutorId);

	/// <summary>
	/// Lấy danh sách TutorSubject theo TutorId
	/// </summary>
	public async Task<IReadOnlyList<TutorSubject>> GetByTutorIdAsync(int tutorId)
		=> await IncludeAll().Where(t => t.TutorId == tutorId).ToListAsync();

	/// <summary>
	/// Lấy danh sách TutorSubject theo SubjectId
	/// </summary>
	public async Task<IReadOnlyList<TutorSubject>> GetBySubjectIdAsync(int subjectId)
		=> await IncludeAll().Where(t => t.SubjectId == subjectId).ToListAsync();

	/// <summary>
	/// Lấy danh sách TutorSubject theo LevelId
	/// </summary>
	public async Task<IReadOnlyList<TutorSubject>> GetByLevelIdAsync(int levelId)
		=> await IncludeAll().Where(t => t.LevelId == levelId).ToListAsync();

	/// <summary>
	/// Lấy danh sách TutorSubject theo khoảng giá giờ
	/// </summary>
	public async Task<IReadOnlyList<TutorSubject>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate)
		=> await IncludeAll().Where(t => t.HourlyRate.HasValue && t.HourlyRate.Value >= minRate && t.HourlyRate.Value <= maxRate).ToListAsync();

	/// <summary>
	/// Lấy danh sách TutorSubject theo SubjectId và LevelId
	/// </summary>
	public async Task<IReadOnlyList<TutorSubject>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId)
		=> await IncludeAll().Where(t => t.SubjectId == subjectId && t.LevelId == levelId).ToListAsync();

	/// <summary>
	/// Lấy tất cả TutorSubject với đầy đủ thông tin
	/// </summary>
	public async Task<IReadOnlyList<TutorSubject>> GetAllFullAsync()
		=> await IncludeAll().ToListAsync();

	/// <summary>
	/// Thêm TutorSubject mới
	/// </summary>
	public async Task AddAsync(TutorSubject entity)
	{
		await _ctx.TutorSubjects.AddAsync(entity);
		await _ctx.SaveChangesAsync();
	}

	/// <summary>
	/// Cập nhật TutorSubject
	/// </summary>
	public async Task UpdateAsync(TutorSubject entity)
	{
		_ctx.TutorSubjects.Update(entity);
		await _ctx.SaveChangesAsync();
	}

	/// <summary>
	/// Xóa TutorSubject theo ID
	/// </summary>
	public async Task RemoveByIdAsync(int id)
	{
		var entity = await _ctx.TutorSubjects.FindAsync(new object?[] { id });
		if (entity != null)
		{
			_ctx.TutorSubjects.Remove(entity);
			await _ctx.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Xóa tất cả TutorSubject theo TutorId
	/// </summary>
	public async Task RemoveByTutorIdAsync(int tutorId)
	{
		var entities = await _ctx.TutorSubjects.Where(t => t.TutorId == tutorId).ToListAsync();
		if (entities.Any())
		{
			_ctx.TutorSubjects.RemoveRange(entities);
			await _ctx.SaveChangesAsync();
		}
	}
	}
}
