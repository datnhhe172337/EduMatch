using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IScheduleCompletionRepository
    {
        Task<ScheduleCompletion?> GetByScheduleIdAsync(int scheduleId);
        Task<List<ScheduleCompletion>> GetPendingAutoCompleteAsync(DateTime cutoffUtc);
        Task AddAsync(ScheduleCompletion entity);
        void Update(ScheduleCompletion entity);
    }
}
