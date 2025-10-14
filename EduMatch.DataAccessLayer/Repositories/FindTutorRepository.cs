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
            string? keyword, string? gender, string? city, string? teachingMode, int? statusId, int page, int pageSize)
        {
            var query = _context.TutorProfiles
                .Include(t => t.UserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                        .ThenInclude(up => up.SubDistrict)
                            .ThenInclude(sd => sd.Province)
                .Include(t => t.UserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                        .ThenInclude(up => up.City)
                .AsQueryable();

            // Keyword filter
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.UserEmailNavigation.UserName != null && t.UserEmailNavigation.UserName.Contains(keyword)) ||
                    (t.Bio != null && t.Bio.Contains(keyword)) ||
                    (t.TeachingExp != null && t.TeachingExp.Contains(keyword)));
            }

            // Gender (byte)
            if (!string.IsNullOrEmpty(gender))
            {
                if (byte.TryParse(gender, out var genderByte))
                {
                    query = query.Where(t =>
                        t.UserEmailNavigation.UserProfile != null &&
                        t.UserEmailNavigation.UserProfile.Gender == genderByte);
                }
                else if (gender.Equals("male", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.UserEmailNavigation.UserProfile!.Gender == 0);
                }
                else if (gender.Equals("female", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.UserEmailNavigation.UserProfile!.Gender == 1);
                }
            }

            // City filter
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(t =>
                    (t.UserEmailNavigation.UserProfile.City != null &&
                     t.UserEmailNavigation.UserProfile.City.Name.Contains(city)) ||
                    (t.UserEmailNavigation.UserProfile.SubDistrict != null &&
                     t.UserEmailNavigation.UserProfile.SubDistrict.Province != null &&
                     t.UserEmailNavigation.UserProfile.SubDistrict.Province.Name.Contains(city)));
            }

            // Teaching mode (byte)
            if (!string.IsNullOrEmpty(teachingMode) && byte.TryParse(teachingMode, out var modeByte))
            {
                query = query.Where(t => t.TeachingModes == modeByte);
            }

            // Status (byte)
            if (statusId.HasValue)
            {
                query = query.Where(t => t.Status == (byte)statusId.Value);
            }

            // Pagination
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

