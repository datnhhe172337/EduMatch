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

        // --- METHOD 1: For Read-Only Queries (Keeps AsNoTracking) ---
        private IQueryable<TutorProfile> IncludeAllReadOnly() =>
            _ctx.TutorProfiles
            .AsNoTracking() 
            .AsSplitQuery()
            .Include(t => t.TutorAvailabilities)
                .ThenInclude(a => a.Slot)
            .Include(t => t.TutorCertificates)
                .ThenInclude(c => c.CertificateType)
            .Include(t => t.TutorEducations)
                .ThenInclude(e => e.Institution)
            .Include(t => t.TutorSubjects)
                .ThenInclude(ts => ts.Subject)
            .Include(t => t.TutorSubjects)
                .ThenInclude(ts => ts.Level)
            .Include(t => t.UserEmailNavigation)
                .ThenInclude(u => u!.UserProfile) 
                    .ThenInclude(p => p!.City) 
            .Include(t => t.UserEmailNavigation)
                .ThenInclude(u => u!.UserProfile) 
                    .ThenInclude(p => p!.SubDistrict); 

        // --- METHOD 2: For Update Queries (NO AsNoTracking) ---
        private IQueryable<TutorProfile> IncludeAllForUpdate() =>
            _ctx.TutorProfiles
            .AsSplitQuery()
            .Include(t => t.TutorAvailabilities)
                .ThenInclude(a => a.Slot)
            .Include(t => t.TutorCertificates)
                .ThenInclude(c => c.CertificateType)
            .Include(t => t.TutorEducations)
                .ThenInclude(e => e.Institution)
            .Include(t => t.TutorSubjects)
                .ThenInclude(ts => ts.Subject)
            .Include(t => t.TutorSubjects)
                .ThenInclude(ts => ts.Level)
            .Include(t => t.UserEmailNavigation)
                .ThenInclude(u => u!.UserProfile) 
                    .ThenInclude(p => p!.City)
            .Include(t => t.UserEmailNavigation)
                .ThenInclude(u => u!.UserProfile) 
                    .ThenInclude(p => p!.SubDistrict); 


        public async Task<TutorProfile?> GetByIdFullAsync(int id, CancellationToken ct = default)
            => await IncludeAllReadOnly().FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<TutorProfile?> GetByEmailFullAsync(string email, CancellationToken ct = default)
            => await IncludeAllReadOnly().FirstOrDefaultAsync(t => t.UserEmail == email, ct);
        //public async Task<TutorProfile?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default)
        //    => await IncludeAll().FirstOrDefaultAsync(t => t.Id == tutorId, ct);
        public async Task<IReadOnlyList<TutorProfile>> GetAllFullAsync(CancellationToken ct = default)
            => await IncludeAllReadOnly().ToListAsync();

        // --- Get Methods For Update (Use ForUpdate - WITHOUT AsNoTracking) ---
        public async Task<TutorProfile?> GetByIdForUpdateAsync(int id, CancellationToken ct = default)
             => await IncludeAllForUpdate().FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<TutorProfile?> GetByEmailForUpdateAsync(string email, CancellationToken ct = default)
             => await IncludeAllForUpdate().FirstOrDefaultAsync(t => t.UserEmail == email, ct);

        // --- Simple GetById (WITH Tracking, NO Includes) ---
        public async Task<TutorProfile?> GetByIdAsync(int id, CancellationToken ct = default)
             => await _ctx.TutorProfiles.FirstOrDefaultAsync(t => t.Id == id, ct); // WITH tracking

        // --- Add Method ---
        public async Task AddAsync(TutorProfile entity, CancellationToken ct = default)
        {
            await _ctx.TutorProfiles.AddAsync(entity, ct);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(TutorProfile entity, CancellationToken ct = default)
        {
            _ctx.TutorProfiles.Update(entity);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task RemoveByIdAsync(int id, CancellationToken ct = default)
        {
            // FindAsync is simple for removal by PK
            var entity = await _ctx.TutorProfiles.FindAsync(new object?[] { id }, ct);
            if (entity != null)
            {
                _ctx.TutorProfiles.Remove(entity);
                await _ctx.SaveChangesAsync(ct);
            }
        }

    }
}
