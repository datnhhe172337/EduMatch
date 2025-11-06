using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorEducationService
    {
        /// <summary>
        /// Lấy TutorEducation theo ID với đầy đủ thông tin
        /// </summary>
        Task<TutorEducationDto?> GetByIdFullAsync(int id);
        /// <summary>
        /// Lấy TutorEducation theo TutorId với đầy đủ thông tin
        /// </summary>
        Task<TutorEducationDto?> GetByTutorIdFullAsync(int tutorId);
        /// <summary>
        /// Lấy danh sách TutorEducation theo TutorId
        /// </summary>
        Task<IReadOnlyList<TutorEducationDto>> GetByTutorIdAsync(int tutorId);
        /// <summary>
        /// Lấy danh sách TutorEducation theo InstitutionId
        /// </summary>
        Task<IReadOnlyList<TutorEducationDto>> GetByInstitutionIdAsync(int institutionId);
        /// <summary>
        /// Lấy danh sách TutorEducation theo trạng thái xác thực
        /// </summary>
        Task<IReadOnlyList<TutorEducationDto>> GetByVerifiedStatusAsync(VerifyStatus verified);
        /// <summary>
        /// Lấy danh sách TutorEducation đang chờ xác thực
        /// </summary>
        Task<IReadOnlyList<TutorEducationDto>> GetPendingVerificationsAsync();
        /// <summary>
        /// Lấy tất cả TutorEducation với đầy đủ thông tin
        /// </summary>
        Task<IReadOnlyList<TutorEducationDto>> GetAllFullAsync();
        /// <summary>
        /// Tạo TutorEducation mới
        /// </summary>
        Task<TutorEducationDto> CreateAsync(TutorEducationCreateRequest request);
        /// <summary>
        /// Tạo nhiều TutorEducation
        /// </summary>
        Task<List<TutorEducationDto>> CreateBulkAsync(List<TutorEducationCreateRequest> requests);
        /// <summary>
        /// Xóa TutorEducation theo ID
        /// </summary>
        Task DeleteAsync(int id);
        /// <summary>
        /// Xóa tất cả TutorEducation theo TutorId
        /// </summary>
        Task DeleteByTutorIdAsync(int tutorId);
        /// <summary>
        /// Cập nhật TutorEducation
        /// </summary>
        Task<TutorEducationDto> UpdateAsync(TutorEducationUpdateRequest request);
    }
}
