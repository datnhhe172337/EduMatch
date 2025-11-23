using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ITutorRatingSummaryRepository
    {
        Task<TutorRatingSummary?> GetByTutorIdAsync(int tutorId);
        Task AddAsync(TutorRatingSummary summary);
        Task UpdateAsync(TutorRatingSummary summary);
        Task SaveAsync();
    }
}
