using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorEducationRepository
	{
		Task<TutorEducation?> GetByIdFullAsync(int id);
		Task<TutorEducation?> GetByTutorIdFullAsync(int tutorId);
		Task<IReadOnlyList<TutorEducation>> GetByTutorIdAsync(int tutorId);
		Task<IReadOnlyList<TutorEducation>> GetByInstitutionIdAsync(int institutionId);
		Task<IReadOnlyList<TutorEducation>> GetByVerifiedStatusAsync(VerifyStatus verified);
		Task<IReadOnlyList<TutorEducation>> GetPendingVerificationsAsync();
		Task<IReadOnlyList<TutorEducation>> GetAllFullAsync();
		Task AddAsync(TutorEducation entity);
		Task UpdateAsync(TutorEducation entity);
		Task RemoveByIdAsync(int id);
		Task RemoveByTutorIdAsync(int tutorId);
	}
}
