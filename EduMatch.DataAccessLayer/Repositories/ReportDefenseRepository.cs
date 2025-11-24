using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ReportDefenseRepository : IReportDefenseRepository
    {
        private readonly EduMatchContext _context;

        public ReportDefenseRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<ReportDefense> AddAsync(ReportDefense defense)
        {
            _context.ReportDefenses.Add(defense);
            await _context.SaveChangesAsync();
            return defense;
        }

        public async Task<IReadOnlyList<ReportDefense>> GetByReportIdAsync(int reportId)
        {
            return await _context.ReportDefenses
                .Where(d => d.ReportId == reportId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<ReportDefense?> GetByIdAsync(int id)
        {
            return await _context.ReportDefenses.FindAsync(id);
        }

        public async Task<ReportDefense> UpdateAsync(ReportDefense defense)
        {
            _context.ReportDefenses.Update(defense);
            await _context.SaveChangesAsync();
            return defense;
        }

        public async Task DeleteAsync(ReportDefense defense)
        {
            _context.ReportDefenses.Remove(defense);
            await _context.SaveChangesAsync();
        }
    }
}
