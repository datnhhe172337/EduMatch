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
	public sealed class TutorProfileRepository : ITutorProfileRepository
	{
		private readonly EduMatchContext _ctx;
		public TutorProfileRepository(EduMatchContext ctx) => _ctx = ctx;

		private IQueryable<TutorProfile> IncludeAll() =>
	_ctx.TutorProfiles
		.AsSplitQuery()

		// Availabilities + Slot
		.Include(t => t.TutorAvailabilities)
			.ThenInclude(a => a.Slot)

		// Certificates + CertificateType
		.Include(t => t.TutorCertificates)
			.ThenInclude(c => c.CertificateType)

		// Educations + Institution
		.Include(t => t.TutorEducations)
			.ThenInclude(e => e.Institution)

		// Subjects + Subject, Level (cần 2 Include riêng)
		.Include(t => t.TutorSubjects)
			.ThenInclude(ts => ts.Subject)
		.Include(t => t.TutorSubjects)
			.ThenInclude(ts => ts.Level)

		// User -> UserProfile -> City/SubDistrict
		.Include(t => t.UserEmailNavigation)
			.ThenInclude(u => u.UserProfile)
				.ThenInclude(p => p.City)
		.Include(t => t.UserEmailNavigation)
			.ThenInclude(u => u.UserProfile)
				.ThenInclude(p => p.SubDistrict);



		public async Task<TutorProfile?> GetByIdFullAsync(int id)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == id);

		public async Task<TutorProfile?> GetByTutorIdFullAsync(int tutorId)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.Id == tutorId);

		public async Task<TutorProfile?> GetByEmailFullAsync(string email)
			=> await IncludeAll().FirstOrDefaultAsync(t => t.UserEmail == email);

		public async Task<IReadOnlyList<TutorProfile>> GetAllFullAsync()
			=> await IncludeAll().ToListAsync();

		public async Task AddAsync(TutorProfile entity)
		{
			await _ctx.TutorProfiles.AddAsync(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task UpdateAsync(TutorProfile entity)
		{
			_ctx.TutorProfiles.Update(entity);
			await _ctx.SaveChangesAsync();
		}

		public async Task RemoveByIdAsync(int id)
		{
			var entity = await _ctx.TutorProfiles.FindAsync(new object?[] { id });
			if (entity != null)
			{
				_ctx.TutorProfiles.Remove(entity);
				await _ctx.SaveChangesAsync();
			}
		}

	}
}
