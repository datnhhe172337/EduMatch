using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.DataAccessLayer.Entities;
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
		/// Xác thực TutorProfile
		/// </summary>
		Task<TutorProfileDto> VerifyAsync(int id, string verifiedBy);


        //Task<List<TutorProfileDto>> GetTutorsUpdatedAfterAsync(DateTime lastSync);
        Task<IReadOnlyList<TutorProfileDto>> GetTutorsUpdatedAfterAsync(DateTime lastSync);

    }
}
