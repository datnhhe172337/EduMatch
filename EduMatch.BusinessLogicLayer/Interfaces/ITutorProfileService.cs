using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITutorProfileService
	{
		Task<TutorProfileDto?> GetByIdFullAsync(int id);
		Task<TutorProfileDto?> GetByEmailFullAsync(string email);
		Task<IReadOnlyList<TutorProfileDto>> GetAllFullAsync();

		Task<TutorProfileDto> CreateAsync(TutorProfileCreateRequest request);
		Task<TutorProfileDto> UpdateAsync(TutorProfileUpdateRequest request);
		Task DeleteAsync(int id);
		Task<TutorProfileDto> VerifyAsync(int id, string verifiedBy);
	}
}
