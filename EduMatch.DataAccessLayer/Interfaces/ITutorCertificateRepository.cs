using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorCertificateRepository
	{
		Task<TutorCertificate?> GetByIdFullAsync(int id);
		Task<TutorCertificate?> GetByTutorIdFullAsync(int tutorId);
		Task<IReadOnlyList<TutorCertificate>> GetByTutorIdAsync(int tutorId);
		Task<IReadOnlyList<TutorCertificate>> GetByCertificateTypeAsync(int certificateTypeId);
		Task<IReadOnlyList<TutorCertificate>> GetByVerifiedStatusAsync(VerifyStatus verified);
		Task<IReadOnlyList<TutorCertificate>> GetExpiredCertificatesAsync();
		Task<IReadOnlyList<TutorCertificate>> GetExpiringCertificatesAsync(DateTime beforeDate);
		Task<IReadOnlyList<TutorCertificate>> GetAllFullAsync();
		Task AddAsync(TutorCertificate entity);
		Task UpdateAsync(TutorCertificate entity);
		Task RemoveByIdAsync(int id);
		Task RemoveByTutorIdAsync(int tutorId);
	}
}
