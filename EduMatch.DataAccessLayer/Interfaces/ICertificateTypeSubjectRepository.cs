using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ICertificateTypeSubjectRepository
	{
		Task<CertificateTypeSubject?> GetByIdAsync(int id);
		Task<IReadOnlyList<CertificateTypeSubject>> GetByCertificateTypeIdAsync(int certificateTypeId);
		Task<IReadOnlyList<CertificateTypeSubject>> GetBySubjectIdAsync(int subjectId);
		Task<CertificateTypeSubject?> GetByCertificateTypeAndSubjectAsync(int certificateTypeId, int subjectId);
		Task<IReadOnlyList<CertificateTypeSubject>> GetAllAsync();
		Task AddAsync(CertificateTypeSubject entity);
		Task UpdateAsync(CertificateTypeSubject entity);
		Task RemoveByIdAsync(int id);
		Task AddRangeAsync(IEnumerable<CertificateTypeSubject> entities);
		Task RemoveByCertificateTypeIdAsync(int certificateTypeId);
	}
}
