using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorProfileRepository
	{
		/// <summary>
		/// Lấy TutorProfile theo ID với đầy đủ thông tin
		/// </summary>
		Task<TutorProfile?> GetByIdFullAsync(int id);
		/// <summary>
		/// Lấy TutorProfile theo Email với đầy đủ thông tin
		/// </summary>
		Task<TutorProfile?> GetByEmailFullAsync(string email);
		/// <summary>
		/// Lấy tất cả TutorProfile với đầy đủ thông tin
		/// </summary>
		Task<IReadOnlyList<TutorProfile>> GetAllFullAsync();
		/// <summary>
		/// Thêm TutorProfile mới
		/// </summary>
		Task AddAsync(TutorProfile entity);
		/// <summary>
		/// Cập nhật TutorProfile
		/// </summary>
		Task UpdateAsync(TutorProfile entity);
		/// <summary>
		/// Xóa TutorProfile theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);

        //Task<List<TutorProfile>> GetTutorsUpdatedAfterAsync(DateTime lastSync);
        Task<IReadOnlyList<TutorProfile>> GetTutorsUpdatedAfterAsync(DateTime lastSync);

		Task SaveChangesAsync();
    }
}
