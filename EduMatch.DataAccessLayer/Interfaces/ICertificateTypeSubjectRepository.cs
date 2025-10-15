using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ICertificateTypeSubjectRepository
	{
		Task<CertificateTypeSubject?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<IReadOnlyList<CertificateTypeSubject>> GetByCertificateTypeIdAsync(int certificateTypeId, CancellationToken ct = default);
		Task<IReadOnlyList<CertificateTypeSubject>> GetBySubjectIdAsync(int subjectId, CancellationToken ct = default);
		Task<CertificateTypeSubject?> GetByCertificateTypeAndSubjectAsync(int certificateTypeId, int subjectId, CancellationToken ct = default);
		Task<IReadOnlyList<CertificateTypeSubject>> GetAllAsync(CancellationToken ct = default);
		Task AddAsync(CertificateTypeSubject entity, CancellationToken ct = default);
		Task UpdateAsync(CertificateTypeSubject entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
