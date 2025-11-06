using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ICertificateTypeRepository
	{
		/// <summary>
		/// Lấy CertificateType theo ID
		/// </summary>
		Task<CertificateType?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy CertificateType theo Code
		/// </summary>
		Task<CertificateType?> GetByCodeAsync(string code);
		/// <summary>
		/// Lấy tất cả CertificateType
		/// </summary>
		Task<IReadOnlyList<CertificateType>> GetAllAsync();
		/// <summary>
		/// Tìm CertificateType theo tên
		/// </summary>
		Task<IReadOnlyList<CertificateType>> GetByNameAsync(string name);
		/// <summary>
		/// Thêm CertificateType mới
		/// </summary>
		Task AddAsync(CertificateType entity);
		/// <summary>
		/// Cập nhật CertificateType
		/// </summary>
		Task UpdateAsync(CertificateType entity);
		/// <summary>
		/// Xóa CertificateType theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
	}
}
