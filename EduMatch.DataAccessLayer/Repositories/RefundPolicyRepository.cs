using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class RefundPolicyRepository : IRefundPolicyRepository
    {
        private readonly EduMatchContext _context;

        public RefundPolicyRepository(EduMatchContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả RefundPolicy với lọc theo IsActive
        /// </summary>
        public async Task<IEnumerable<RefundPolicy>> GetAllAsync(bool? isActive = null)
        {
            var query = _context.RefundPolicies.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(rp => rp.IsActive == isActive.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy RefundPolicy theo ID
        /// </summary>
        public async Task<RefundPolicy?> GetByIdAsync(int id)
        {
            return await _context.RefundPolicies.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Tạo RefundPolicy mới
        /// </summary>
        public async Task CreateAsync(RefundPolicy entity)
        {
            await _context.RefundPolicies.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật RefundPolicy
        /// </summary>
        public async Task UpdateAsync(RefundPolicy entity)
        {
            _context.RefundPolicies.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa RefundPolicy theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.RefundPolicies.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                _context.RefundPolicies.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}

