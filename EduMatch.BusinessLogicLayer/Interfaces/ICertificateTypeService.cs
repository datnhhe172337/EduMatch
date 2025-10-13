using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ICertificateTypeService
	{
		Task<CertificateTypeDto?> GetByIdAsync(int id);
		Task<CertificateTypeDto?> GetByCodeAsync(string code);
		Task<IReadOnlyList<CertificateTypeDto>> GetAllAsync();
		Task<IReadOnlyList<CertificateTypeDto>> GetByNameAsync(string name);
		Task<CertificateTypeDto> CreateAsync(CertificateTypeCreateRequest request);
		Task<CertificateTypeDto> UpdateAsync(CertificateTypeUpdateRequest request);
		Task DeleteAsync(int id);
	}
}
