using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ICertificateTypeRepository
	{
		Task<CertificateType?> GetByIdAsync(int id);
		Task<CertificateType?> GetByCodeAsync(string code);
		Task<IReadOnlyList<CertificateType>> GetAllAsync();
		Task<IReadOnlyList<CertificateType>> GetByNameAsync(string name);
		Task AddAsync(CertificateType entity);
		Task UpdateAsync(CertificateType entity);
		Task RemoveByIdAsync(int id);
	}
}
