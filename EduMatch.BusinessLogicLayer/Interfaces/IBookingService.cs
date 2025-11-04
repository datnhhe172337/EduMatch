using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Booking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IBookingService
    {
        Task<List<BookingDto>> GetAllByLearnerEmailAsync(string email, int? status, int? tutorSubjectId, int page = 1, int pageSize = 10);
        Task<int> CountByLearnerEmailAsync(string email, int? status, int? tutorSubjectId);
        Task<List<BookingDto>> GetAllByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId, int page = 1, int pageSize = 10);
        Task<int> CountByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId);
        Task<BookingDto?> GetByIdAsync(int id);
        Task<BookingDto> CreateAsync(BookingCreateRequest request);
        Task<BookingDto> UpdateAsync(BookingUpdateRequest request);
        Task DeleteAsync(int id);
    }
}
