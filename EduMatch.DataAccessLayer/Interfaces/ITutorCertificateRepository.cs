using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorCertificateRepository
	{
		Task<TutorCertificate?> GetByIdFullAsync(int id, CancellationToken ct = default);
		Task<TutorCertificate?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorCertificate>> GetByTutorIdAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorCertificate>> GetByCertificateTypeAsync(int certificateTypeId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorCertificate>> GetByVerifiedStatusAsync(VerifyStatus verified, CancellationToken ct = default);
		Task<IReadOnlyList<TutorCertificate>> GetExpiredCertificatesAsync(CancellationToken ct = default);
		Task<IReadOnlyList<TutorCertificate>> GetExpiringCertificatesAsync(DateTime beforeDate, CancellationToken ct = default);
		Task<IReadOnlyList<TutorCertificate>> GetAllFullAsync(CancellationToken ct = default);
		Task AddAsync(TutorCertificate entity, CancellationToken ct = default);
		Task UpdateAsync(TutorCertificate entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
		Task RemoveByTutorIdAsync(int tutorId, CancellationToken ct = default);
	}
}
