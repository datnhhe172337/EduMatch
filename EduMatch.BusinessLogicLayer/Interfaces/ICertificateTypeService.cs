using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ICertificateTypeService
	{
		/// <summary>
		/// Lấy CertificateType theo ID
		/// </summary>
		Task<CertificateTypeDto?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy CertificateType theo Code
		/// </summary>
		Task<CertificateTypeDto?> GetByCodeAsync(string code);
		/// <summary>
		/// Lấy tất cả CertificateType
		/// </summary>
		Task<IReadOnlyList<CertificateTypeDto>> GetAllAsync();
		/// <summary>
		/// Tìm CertificateType theo tên
		/// </summary>
		Task<IReadOnlyList<CertificateTypeDto>> GetByNameAsync(string name);
		/// <summary>
		/// Tạo CertificateType mới
		/// </summary>
		Task<CertificateTypeDto> CreateAsync(CertificateTypeCreateRequest request);
		/// <summary>
		/// Cập nhật CertificateType
		/// </summary>
		Task<CertificateTypeDto> UpdateAsync(CertificateTypeUpdateRequest request);
		/// <summary>
		/// Xóa CertificateType theo ID
		/// </summary>
		Task DeleteAsync(int id);
		/// <summary>
		/// Xác thực CertificateType
		/// </summary>
		Task<CertificateTypeDto> VerifyAsync(int id, string verifiedBy);
		/// <summary>
		/// Thêm danh sách Subject vào CertificateType
		/// </summary>
		Task<CertificateTypeDto> AddSubjectsToCertificateTypeAsync(int certificateTypeId, List<int> subjectIds);
	}
}
