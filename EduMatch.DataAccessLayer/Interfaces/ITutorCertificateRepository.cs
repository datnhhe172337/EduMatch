using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorCertificateRepository
	{
		/// <summary>
		/// Lấy TutorCertificate theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorCertificate?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy TutorCertificate theo TutorId với đầy đủ thông tin
		/// </summary>
		Task<TutorCertificate?> GetByTutorIdFullAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorCertificate theo TutorId
		/// </summary>
		Task<IReadOnlyList<TutorCertificate>> GetByTutorIdAsync(int tutorId);
		/// <summary>
		/// Lấy danh sách TutorCertificate theo CertificateTypeId
		/// </summary>
		Task<IReadOnlyList<TutorCertificate>> GetByCertificateTypeAsync(int certificateTypeId);
		/// <summary>
		/// Lấy danh sách TutorCertificate theo trạng thái xác thực
		/// </summary>
		Task<IReadOnlyList<TutorCertificate>> GetByVerifiedStatusAsync(VerifyStatus verified);
		/// <summary>
		/// Lấy danh sách TutorCertificate đã hết hạn
		/// </summary>
		Task<IReadOnlyList<TutorCertificate>> GetExpiredCertificatesAsync();
		/// <summary>
		/// Lấy danh sách TutorCertificate sắp hết hạn
		/// </summary>
		Task<IReadOnlyList<TutorCertificate>> GetExpiringCertificatesAsync(DateTime beforeDate);
		/// <summary>
		/// Lấy tất cả TutorCertificate với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorCertificate>> GetAllFullAsync();
		/// <summary>
		/// Thêm TutorCertificate mới
		/// </summary>
		Task AddAsync(TutorCertificate entity);
		/// <summary>
		/// Cập nhật TutorCertificate
		/// </summary>
		Task UpdateAsync(TutorCertificate entity);
		/// <summary>
		/// Xóa TutorCertificate theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
		/// <summary>
		/// Xóa tất cả TutorCertificate theo TutorId
		/// </summary>
		Task RemoveByTutorIdAsync(int tutorId);
	}
}
