using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorAvailabilityService
    {
        /// <summary>
        /// Lấy TutorAvailability theo ID với đầy đủ thông tin
        /// </summary>
        Task<TutorAvailabilityDto?> GetByIdFullAsync(int id);
        /// <summary>
        /// Lấy danh sách TutorAvailability theo TutorId
        /// </summary>
        Task<IReadOnlyList<TutorAvailabilityDto>> GetByTutorIdAsync(int tutorId);
        /// <summary>
        /// Lấy danh sách TutorAvailability theo TutorId với đầy đủ thông tin
        /// </summary>
        Task<IReadOnlyList<TutorAvailabilityDto>> GetByTutorIdFullAsync(int tutorId);
        /// <summary>
        /// Lấy tất cả TutorAvailability với đầy đủ thông tin
        /// </summary>
        Task<IReadOnlyList<TutorAvailabilityDto>> GetAllFullAsync();
        /// <summary>
        /// Tạo TutorAvailability mới
        /// </summary>
        Task<TutorAvailabilityDto> CreateAsync(TutorAvailabilityCreateRequest request);
        /// <summary>
        /// Tạo nhiều TutorAvailability
        /// </summary>
        Task<List<TutorAvailabilityDto>> CreateBulkAsync(List<TutorAvailabilityCreateRequest> requests);
        /// <summary>
        /// Xóa TutorAvailability theo ID
        /// </summary>
        Task DeleteAsync(int id);
        /// <summary>
        /// Cập nhật TutorAvailability
        /// </summary>
        Task<TutorAvailabilityDto> UpdateAsync(TutorAvailabilityUpdateRequest request);
        /// <summary>
        /// Cập nhật trạng thái của TutorAvailability
        /// </summary>
        Task<TutorAvailabilityDto> UpdateStatusAsync(int id, TutorAvailabilityStatus status);
    }
}