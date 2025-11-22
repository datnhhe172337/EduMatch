using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class TutorVerificationRequestRepository : ITutorVerificationRequestRepository
    {
        private readonly EduMatchContext _context;

        public TutorVerificationRequestRepository(EduMatchContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả TutorVerificationRequest với lọc theo Status
        /// </summary>
        public async Task<IEnumerable<TutorVerificationRequest>> GetAllByStatusAsync(int? status = null)
        {
            var query = _context.TutorVerificationRequests
                .Include(tvr => tvr.Tutor)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(tvr => tvr.Status == status.Value);
            }

            return await query
                .OrderByDescending(tvr => tvr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả TutorVerificationRequest theo Email hoặc TutorId với lọc theo Status
        /// </summary>
        public async Task<IEnumerable<TutorVerificationRequest>> GetAllByEmailOrTutorIdAsync(string? email = null, int? tutorId = null, int? status = null)
        {
            var query = _context.TutorVerificationRequests
                .Include(tvr => tvr.Tutor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(tvr => tvr.UserEmail == email);
            }

            if (tutorId.HasValue)
            {
                query = query.Where(tvr => tvr.TutorId == tutorId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(tvr => tvr.Status == status.Value);
            }

            return await query
                .OrderByDescending(tvr => tvr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy TutorVerificationRequest theo ID
        /// </summary>
        public async Task<TutorVerificationRequest?> GetByIdAsync(int id)
        {
            return await _context.TutorVerificationRequests
                .Include(tvr => tvr.Tutor)
                .FirstOrDefaultAsync(tvr => tvr.Id == id);
        }

        /// <summary>
        /// Tạo TutorVerificationRequest mới
        /// </summary>
        public async Task CreateAsync(TutorVerificationRequest entity)
        {
            await _context.TutorVerificationRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật TutorVerificationRequest
        /// </summary>
        public async Task UpdateAsync(TutorVerificationRequest entity)
        {
            _context.TutorVerificationRequests.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa TutorVerificationRequest theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.TutorVerificationRequests.FirstOrDefaultAsync(tvr => tvr.Id == id);
            if (entity != null)
            {
                _context.TutorVerificationRequests.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}

