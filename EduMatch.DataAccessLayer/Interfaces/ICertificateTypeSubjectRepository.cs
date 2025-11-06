using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ICertificateTypeSubjectRepository
	{
		/// <summary>
		/// Lấy CertificateTypeSubject theo ID
		/// </summary>
		Task<CertificateTypeSubject?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy danh sách CertificateTypeSubject theo CertificateTypeId
		/// </summary>
		Task<IReadOnlyList<CertificateTypeSubject>> GetByCertificateTypeIdAsync(int certificateTypeId);
		/// <summary>
		/// Lấy danh sách CertificateTypeSubject theo SubjectId
		/// </summary>
		Task<IReadOnlyList<CertificateTypeSubject>> GetBySubjectIdAsync(int subjectId);
		/// <summary>
		/// Lấy CertificateTypeSubject theo CertificateTypeId và SubjectId
		/// </summary>
		Task<CertificateTypeSubject?> GetByCertificateTypeAndSubjectAsync(int certificateTypeId, int subjectId);
		/// <summary>
		/// Lấy tất cả CertificateTypeSubject
		/// </summary>
		Task<IReadOnlyList<CertificateTypeSubject>> GetAllAsync();
		/// <summary>
		/// Thêm CertificateTypeSubject mới
		/// </summary>
		Task AddAsync(CertificateTypeSubject entity);
		/// <summary>
		/// Cập nhật CertificateTypeSubject
		/// </summary>
		Task UpdateAsync(CertificateTypeSubject entity);
		/// <summary>
		/// Xóa CertificateTypeSubject theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
		/// <summary>
		/// Thêm nhiều CertificateTypeSubject
		/// </summary>
		Task AddRangeAsync(IEnumerable<CertificateTypeSubject> entities);
		/// <summary>
		/// Xóa tất cả CertificateTypeSubject theo CertificateTypeId
		/// </summary>
		Task RemoveByCertificateTypeIdAsync(int certificateTypeId);
	}
}
