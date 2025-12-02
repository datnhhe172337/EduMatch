using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITutorProfileService
	{
		/// <summary>
		/// Lấy TutorProfile theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorProfileDto?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy TutorProfile theo Email với đầy đủ thông tin
		/// </summary>
		Task<TutorProfileDto?> GetByEmailFullAsync(string email);
		/// <summary>
		/// Lấy tất cả TutorProfile với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorProfileDto>> GetAllFullAsync();
		/// <summary>
		/// Tạo TutorProfile mới
		/// </summary>
		Task<TutorProfileDto> CreateAsync(TutorProfileCreateRequest request);
		/// <summary>
		/// Cập nhật TutorProfile
		/// </summary>
		Task<TutorProfileDto> UpdateAsync(TutorProfileUpdateRequest request);
		/// <summary>
		/// Xóa TutorProfile theo ID
		/// </summary>
		Task DeleteAsync(int id);
		/// <summary>
		/// Cập nhật Status của TutorProfile (chỉ cho phép từ Approved sang Suspended hoặc Deactivated)
		/// </summary>

		Task<TutorProfileDto> VerifyAsync(int id, string verifiedBy);


        //Task<List<TutorProfileDto>> GetTutorsUpdatedAfterAsync(DateTime lastSync);
        Task<IReadOnlyList<TutorProfileDto>> GetTutorsUpdatedAfterAsync(DateTime lastSync);

		Task<int> SyncAllTutorsAsync();
        Task<TutorProfileDto> UpdateStatusAsync(int id, TutorStatus status);

		Task<List<(TutorProfileDto Tutor, float Score)>> SearchByKeywordAsync(string keyword);

    }

		
}

