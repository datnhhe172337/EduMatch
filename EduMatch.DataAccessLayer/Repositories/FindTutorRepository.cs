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
             string? city,
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

            // ✅ Apply filters only when provided
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

            if (!string.IsNullOrEmpty(city))
                query = query.Where(t => t.UserEmailNavigation.UserProfile.City.Id.ToString() == city);

            if (teachingMode.HasValue)
                query = query.Where(t => t.TeachingModes == teachingMode.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            // ✅ Pagination
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return await query.ToListAsync();
        }
    }
}

