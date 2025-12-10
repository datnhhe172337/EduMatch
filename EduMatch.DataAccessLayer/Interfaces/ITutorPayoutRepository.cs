using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ITutorPayoutRepository
    {
        Task<List<TutorPayout>> GetReadyForPayoutAsync(DateOnly payoutDate);
        Task<TutorPayout?> GetByScheduleIdAsync(int scheduleId);
        Task<List<TutorPayout>> GetByBookingIdAsync(int bookingId);
        Task AddAsync(TutorPayout entity);
        void Update(TutorPayout entity);
    }
}
