using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorRatingSummaryService
    {
        Task<TutorRatingSummary> EnsureAndUpdateSummaryAsync(int tutorId);
        Task<TutorRatingSummary?> GetByTutorIdAsync(int tutorId);
        Task<TutorRatingSummary> AddRatingSummary(int tutorId);
    }
}
