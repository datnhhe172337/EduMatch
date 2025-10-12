using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Data;
using EduMatch.DataAccessLayer.Entities;

using Microsoft.EntityFrameworkCore;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class FindTutorService : IFindTutorService
    {
        private readonly EduMatchContext _context;

        public FindTutorService(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TutorProfile>> GetAllTutorsAsync()
        {
            return await _context.TutorProfiles
                .Include(t => t.UserEmailNavigation)
                .Include(t => t.Status)
                
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TutorProfile>> SearchTutorsAsync(TutorFilterDto filter)
        {
            var query = _context.TutorProfiles
                .Include(t => t.UserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                        .ThenInclude(up => up.SubDistrict)
                            .ThenInclude(sd => sd.Province)
                .Include(t => t.Status)
                .AsQueryable();

            // Keyword (matches tutor name, title, or bio)
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(t =>
                    t.UserEmailNavigation.UserName.Contains(filter.Keyword) ||
                    t.Title.Contains(filter.Keyword) ||
                    t.Bio.Contains(filter.Keyword));
            }

            // Gender
            if (!string.IsNullOrEmpty(filter.Gender))
                query = query.Where(t => t.Gender == filter.Gender);

            // Province (City filter)
            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(t =>
                    t.UserEmailNavigation.UserProfile.SubDistrict.Province.Name.Contains(filter.City));
            }

            // Teaching mode (Online, Offline, Hybrid, etc.)
            if (!string.IsNullOrEmpty(filter.TeachingMode))
                query = query.Where(t => t.TeachingModes.Contains(filter.TeachingMode));

            // Status
            if (filter.StatusId.HasValue)
                query = query.Where(t => t.StatusId == filter.StatusId.Value);

            // Pagination
            query = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);

            return await query.ToListAsync();
        }


    }
}
