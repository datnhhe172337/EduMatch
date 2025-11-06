using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class FindTutorRepository : IFindTutorRepository
    {
        private readonly EduMatchContext _context;

        public FindTutorRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TutorProfile>> GetAllTutorsAsync()
        {
            return await _context.TutorProfiles
                .Include(t => t.UserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                        .ThenInclude(up => up.SubDistrict)
                            .ThenInclude(sd => sd.Province)
                .Include(t => t.UserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                        .ThenInclude(up => up.City)
                .Include(t => t.TutorAvailabilities)
                .Include(t => t.TutorCertificates)
                .Include(t => t.TutorEducations)
                .Include(t => t.TutorSubjects)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TutorProfile>> SearchTutorsAsync(
            string? keyword,
            Gender? gender,
            int? cityId,
            TeachingMode? teachingMode,
            TutorStatus? status,
            int page,
            int pageSize)
        {
            var query = _context.TutorProfiles
                .Include(t => t.UserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                        .ThenInclude(up => up.SubDistrict)
                            .ThenInclude(sd => sd.Province)
                .Include(t => t.UserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                        .ThenInclude(up => up.City)
                .Include(t => t.TutorAvailabilities)
                .Include(t => t.TutorCertificates)
                .Include(t => t.TutorEducations)
                .Include(t => t.TutorSubjects)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.UserEmailNavigation.UserName != null && t.UserEmailNavigation.UserName.Contains(keyword)) ||
                    (t.Bio != null && t.Bio.Contains(keyword)) ||
                    (t.TeachingExp != null && t.TeachingExp.Contains(keyword)));
            }

			if (gender.HasValue)
				query = query.Where(t => t.UserEmailNavigation.UserProfile!.Gender == (int)gender.Value);

            if (cityId.HasValue)
                query = query.Where(t => t.UserEmailNavigation.UserProfile.CityId == cityId.Value);

			if (teachingMode.HasValue)
				query = query.Where(t => t.TeachingModes == (int)teachingMode.Value);

			if (status.HasValue)
				query = query.Where(t => t.Status == (int)status.Value);

            query = query.OrderByDescending(t => t.CreatedAt)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize);

            return await query.ToListAsync();
        }
    }
}
