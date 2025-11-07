using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ManageTutorProfileRepository : IManageTutorProfileRepository
    {
        private readonly EduMatchContext _context;
        private readonly ILogger<ManageTutorProfileRepository> _logger;

        public ManageTutorProfileRepository(EduMatchContext context, ILogger<ManageTutorProfileRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TutorProfile?> GetByEmailAsync(string email)
        {
            return await _context.TutorProfiles
        .AsSplitQuery() // <-- Highly recommended to prevent slow queries
        .Include(tp => tp.UserEmailNavigation)
            .ThenInclude(u => u.UserProfile)
                .ThenInclude(p => p.City)
        .Include(tp => tp.UserEmailNavigation)
            .ThenInclude(u => u.UserProfile)
                .ThenInclude(p => p.SubDistrict)
        .Include(tp => tp.TutorSubjects)
            .ThenInclude(ts => ts.Subject)
        .Include(tp => tp.TutorSubjects)
            .ThenInclude(ts => ts.Level)
        .Include(tp => tp.TutorAvailabilities)
            .ThenInclude(a => a.Slot)
        .Include(tp => tp.TutorCertificates)
            .ThenInclude(c => c.CertificateType)
        .Include(tp => tp.TutorEducations)
            .ThenInclude(e => e.Institution)
        .Include(tp => tp.FavoriteTutors)
            .ThenInclude(ft => ft.UserEmailNavigation)
        .FirstOrDefaultAsync(tp => tp.UserEmail == email);
        }


        public async Task<TutorProfile?> GetByIdAsync(int id)
        {
            return await _context.TutorProfiles
                .Include(tp => tp.UserEmailNavigation)
                    .ThenInclude(u => u!.UserProfile)
                .Include(tp => tp.FavoriteTutors)               
                    .ThenInclude(ft => ft.UserEmailNavigation)  
                .FirstOrDefaultAsync(tp => tp.Id == id);
        }


        public async Task<bool> UpdateTutorProfileAsync(string email, TutorProfile updatedProfile, UserProfile updatedUserProfile)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tutorProfile = await _context.TutorProfiles
                    .Include(tp => tp.UserEmailNavigation)
                    .ThenInclude(u => u!.UserProfile)
                    .FirstOrDefaultAsync(tp => tp.UserEmail == email);

                if (tutorProfile?.UserEmailNavigation?.UserProfile == null)
                {
                    _logger.LogWarning("Attempted to update non-existent tutor profile for email: {Email}", email);
                    return false;
                }

                var userProfile = tutorProfile.UserEmailNavigation.UserProfile;

                // Apply updates
                userProfile.AddressLine = updatedUserProfile.AddressLine ?? userProfile.AddressLine;
                userProfile.SubDistrictId = updatedUserProfile.SubDistrictId ?? userProfile.SubDistrictId;
                userProfile.CityId = updatedUserProfile.CityId ?? userProfile.CityId;
                userProfile.AvatarUrl = updatedUserProfile.AvatarUrl ?? userProfile.AvatarUrl;
                userProfile.Gender = updatedUserProfile.Gender ?? userProfile.Gender;
                userProfile.Dob = updatedUserProfile.Dob ?? userProfile.Dob;

                tutorProfile.Bio = updatedProfile.Bio ?? tutorProfile.Bio;
                tutorProfile.TeachingExp = updatedProfile.TeachingExp ?? tutorProfile.TeachingExp;
                tutorProfile.VideoIntroUrl = updatedProfile.VideoIntroUrl ?? tutorProfile.VideoIntroUrl;
                tutorProfile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating tutor profile for {Email}", email);
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
