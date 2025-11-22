using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ReportEvidenceRepository : IReportEvidenceRepository
    {
        private readonly EduMatchContext _context;

        public ReportEvidenceRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<ReportEvidence> AddAsync(ReportEvidence evidence)
        {
            _context.ReportEvidences.Add(evidence);
            await _context.SaveChangesAsync();
            return evidence;
        }

        public async Task<IReadOnlyList<ReportEvidence>> GetByReportIdAsync(int reportId)
        {
            return await _context.ReportEvidences
                .Where(e => e.ReportId == reportId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<ReportEvidence?> GetByIdAsync(int evidenceId)
        {
            return await _context.ReportEvidences.FindAsync(evidenceId);
        }

        public async Task<ReportEvidence> UpdateAsync(ReportEvidence evidence)
        {
            _context.ReportEvidences.Update(evidence);
            await _context.SaveChangesAsync();
            return evidence;
        }

        public async Task DeleteAsync(ReportEvidence evidence)
        {
            _context.ReportEvidences.Remove(evidence);
            await _context.SaveChangesAsync();
        }
    }
}
