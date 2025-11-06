using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITutorCertificateService
	{
		/// <summary>
		/// Lấy TutorCertificate theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorCertificateDto?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy TutorCertificate theo TutorId với đầy đủ thông tin
		/// </summary>
		Task<TutorCertificateDto?> GetByTutorIdFullAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorCertificate theo TutorId
		/// </summary>
		Task<IReadOnlyList<TutorCertificateDto>> GetByTutorIdAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorCertificate theo CertificateTypeId
		/// </summary>
		Task<IReadOnlyList<TutorCertificateDto>> GetByCertificateTypeAsync(int certificateTypeId);
		/// <summary>
		/// Lấy danh sách TutorCertificate theo trạng thái xác thực
		/// </summary>
		Task<IReadOnlyList<TutorCertificateDto>> GetByVerifiedStatusAsync(VerifyStatus verified);
		/// <summary>
		/// Lấy danh sách TutorCertificate đã hết hạn
		/// </summary>
		Task<IReadOnlyList<TutorCertificateDto>> GetExpiredCertificatesAsync();
		/// <summary>
		/// Lấy danh sách TutorCertificate sắp hết hạn
		/// </summary>
		Task<IReadOnlyList<TutorCertificateDto>> GetExpiringCertificatesAsync(DateTime beforeDate);
		/// <summary>
		/// Lấy tất cả TutorCertificate với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorCertificateDto>> GetAllFullAsync();
		/// <summary>
		/// Tạo TutorCertificate mới
		/// </summary>
		Task<TutorCertificateDto> CreateAsync(TutorCertificateCreateRequest request);
		/// <summary>
		/// Cập nhật TutorCertificate
		/// </summary>
		Task<TutorCertificateDto> UpdateAsync(TutorCertificateUpdateRequest request);
		/// <summary>
		/// Tạo nhiều TutorCertificate
		/// </summary>
		Task<List<TutorCertificateDto>> CreateBulkAsync(List<TutorCertificateCreateRequest> requests);
		/// <summary>
		/// Xóa TutorCertificate theo ID
		/// </summary>
		Task DeleteAsync(int id);
		/// <summary>
		/// Xóa tất cả TutorCertificate theo TutorId
		/// </summary>
		Task DeleteByTutorIdAsync(int tutorId);
    }
}

