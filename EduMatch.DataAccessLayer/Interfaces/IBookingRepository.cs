using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllByLearnerEmailAsync(string email, int? status, int? tutorSubjectId, int page, int pageSize);
        Task<int> CountByLearnerEmailAsync(string email, int? status, int? tutorSubjectId);
        Task<IEnumerable<Booking>> GetAllByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId, int page, int pageSize);
        Task<int> CountByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId);
        Task<Booking?> GetByIdAsync(int id);
        Task CreateAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(int id);
    }
}
