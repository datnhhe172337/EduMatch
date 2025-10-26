using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITutorCertificateService
	{
		Task<TutorCertificateDto?> GetByIdFullAsync(int id);
		Task<TutorCertificateDto?> GetByTutorIdFullAsync(int tutorId);
		Task<IReadOnlyList<TutorCertificateDto>> GetByTutorIdAsync(int tutorId);
		Task<IReadOnlyList<TutorCertificateDto>> GetByCertificateTypeAsync(int certificateTypeId);
		Task<IReadOnlyList<TutorCertificateDto>> GetByVerifiedStatusAsync(VerifyStatus verified);
		Task<IReadOnlyList<TutorCertificateDto>> GetExpiredCertificatesAsync();
		Task<IReadOnlyList<TutorCertificateDto>> GetExpiringCertificatesAsync(DateTime beforeDate);
		Task<IReadOnlyList<TutorCertificateDto>> GetAllFullAsync();
		Task<TutorCertificateDto> CreateAsync(TutorCertificateCreateRequest request);
		Task<TutorCertificateDto> UpdateAsync(TutorCertificateUpdateRequest request);
		Task<List<TutorCertificateDto>> CreateBulkAsync(List<TutorCertificateCreateRequest> requests);
		Task DeleteAsync(int id);
		Task DeleteByTutorIdAsync(int tutorId);

    
    }
}

