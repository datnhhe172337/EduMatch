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
                        .ThenInclude(p => p.City)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(t =>
                    (t.UserEmailNavigation.UserName != null && t.UserEmailNavigation.UserName.ToLower().Contains(lowerKeyword)) ||
                    (t.Bio != null && t.Bio.ToLower().Contains(lowerKeyword)) ||
                    (t.TeachingExp != null && t.TeachingExp.ToLower().Contains(lowerKeyword))
                );
            }

            if (gender.HasValue)
                query = query.Where(t => t.UserEmailNavigation.UserProfile.Gender == gender.Value);

            if (cityId.HasValue)
                query = query.Where(t => t.UserEmailNavigation.UserProfile.CityId == cityId.Value);

            if (teachingMode.HasValue)
                query = query.Where(t => t.TeachingModes == teachingMode.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            query = query.OrderByDescending(t => t.CreatedAt)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize);

            return await query.ToListAsync();
        }
    }
}
