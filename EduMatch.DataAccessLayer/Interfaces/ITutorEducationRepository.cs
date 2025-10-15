using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorEducationRepository
	{
		Task<TutorEducation?> GetByIdFullAsync(int id, CancellationToken ct = default);
		Task<TutorEducation?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorEducation>> GetByTutorIdAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorEducation>> GetByInstitutionIdAsync(int institutionId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorEducation>> GetByVerifiedStatusAsync(VerifyStatus verified, CancellationToken ct = default);
		Task<IReadOnlyList<TutorEducation>> GetPendingVerificationsAsync(CancellationToken ct = default);
		Task<IReadOnlyList<TutorEducation>> GetAllFullAsync(CancellationToken ct = default);
		Task AddAsync(TutorEducation entity, CancellationToken ct = default);
		Task UpdateAsync(TutorEducation entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
		Task RemoveByTutorIdAsync(int tutorId, CancellationToken ct = default);
	}
}
