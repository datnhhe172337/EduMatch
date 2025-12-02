using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ITutorVerificationRequestRepository
    {
        /// <summary>
        /// Lấy tất cả TutorVerificationRequest với lọc theo Status
        /// </summary>
        Task<IEnumerable<TutorVerificationRequest>> GetAllByStatusAsync(int? status = null);

        /// <summary>
        /// Lấy tất cả TutorVerificationRequest theo Email hoặc TutorId với lọc theo Status
        /// </summary>
        Task<IEnumerable<TutorVerificationRequest>> GetAllByEmailOrTutorIdAsync(string? email = null, int? tutorId = null, int? status = null);

        /// <summary>
        /// Lấy TutorVerificationRequest theo ID
        /// </summary>
        Task<TutorVerificationRequest?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo TutorVerificationRequest mới
        /// </summary>
        Task CreateAsync(TutorVerificationRequest entity);

        /// <summary>
        /// Cập nhật TutorVerificationRequest
        /// </summary>
        Task UpdateAsync(TutorVerificationRequest entity);

        /// <summary>
        /// Xóa TutorVerificationRequest theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}

