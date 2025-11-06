using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class SystemFeeRepository : ISystemFeeRepository
    {
        private readonly EduMatchContext _context;
        public SystemFeeRepository(EduMatchContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách SystemFee với phân trang
        /// </summary>
        public async Task<IEnumerable<SystemFee>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            return await _context.SystemFees
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả SystemFee (không phân trang)
        /// </summary>
        public async Task<IEnumerable<SystemFee>> GetAllNoPagingAsync()
        {
            return await _context.SystemFees
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Đếm tổng số SystemFee
        /// </summary>
        public async Task<int> CountAsync()
        {
            return await _context.SystemFees.CountAsync();
        }

        /// <summary>
        /// Lấy SystemFee theo ID
        /// </summary>
        public async Task<SystemFee?> GetByIdAsync(int id)
        {
            return await _context.SystemFees.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Tạo SystemFee mới
        /// </summary>
        public async Task CreateAsync(SystemFee entity)
        {
            await _context.SystemFees.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật SystemFee
        /// </summary>
        public async Task UpdateAsync(SystemFee entity)
        {
            _context.SystemFees.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa SystemFee theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.SystemFees.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                _context.SystemFees.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Lấy SystemFee đang hoạt động (IsActive = true, EffectiveFrom <= now, EffectiveTo >= now hoặc null), lấy Id nhỏ nhất
        /// </summary>
        public async Task<SystemFee?> GetActiveSystemFeeAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.SystemFees
                .Where(sf => sf.IsActive == true
                    && sf.EffectiveFrom <= now
                    && (sf.EffectiveTo == null || sf.EffectiveTo >= now))
                .OrderBy(sf => sf.Id)
                .FirstOrDefaultAsync();
        }
    }
}
