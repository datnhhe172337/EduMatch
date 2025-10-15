using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ICertificateTypeRepository
	{
		Task<CertificateType?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<CertificateType?> GetByCodeAsync(string code, CancellationToken ct = default);
		Task<IReadOnlyList<CertificateType>> GetAllAsync(CancellationToken ct = default);
		Task<IReadOnlyList<CertificateType>> GetByNameAsync(string name, CancellationToken ct = default);
		Task AddAsync(CertificateType entity, CancellationToken ct = default);
		Task UpdateAsync(CertificateType entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
