using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorVerificationRequestService
    {
        /// <summary>
        /// Lấy tất cả TutorVerificationRequest với lọc theo Status
        /// </summary>
        Task<List<TutorVerificationRequestDto>> GetAllAsync(TutorVerificationRequestStatus? status = null);

        /// <summary>
        /// Lấy tất cả TutorVerificationRequest theo Email hoặc TutorId với lọc theo Status
        /// </summary>
        Task<List<TutorVerificationRequestDto>> GetAllByEmailOrTutorIdAsync(string? email = null, int? tutorId = null, TutorVerificationRequestStatus? status = null);

        /// <summary>
        /// Lấy TutorVerificationRequest theo ID
        /// </summary>
        Task<TutorVerificationRequestDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo TutorVerificationRequest mới
        /// </summary>
        Task<TutorVerificationRequestDto> CreateAsync(TutorVerificationRequestCreateRequest request);

        /// <summary>
        /// Cập nhật TutorVerificationRequest
        /// </summary>
        Task<TutorVerificationRequestDto> UpdateAsync(TutorVerificationRequestUpdateRequest request);

        /// <summary>
        /// Cập nhật Status của TutorVerificationRequest
        /// </summary>
        Task<TutorVerificationRequestDto> UpdateStatusAsync(int id, TutorVerificationRequestStatus status);
    }
}

