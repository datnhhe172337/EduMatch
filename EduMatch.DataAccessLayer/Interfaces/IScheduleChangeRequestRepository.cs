using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IScheduleChangeRequestRepository
    {
        /// <summary>
        /// Lấy ScheduleChangeRequest theo ID
        /// </summary>
        Task<ScheduleChangeRequest?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo ScheduleChangeRequest mới
        /// </summary>
        Task CreateAsync(ScheduleChangeRequest scheduleChangeRequest);

        /// <summary>
        /// Cập nhật ScheduleChangeRequest
        /// </summary>
        Task UpdateAsync(ScheduleChangeRequest scheduleChangeRequest);

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequesterEmail, sắp xếp theo CreatedAt descending, Id descending
        /// </summary>
        Task<IEnumerable<ScheduleChangeRequest>> GetAllByRequesterEmailAsync(string requesterEmail, int? status = null);

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequestedToEmail, sắp xếp theo CreatedAt descending, Id descending
        /// </summary>
        Task<IEnumerable<ScheduleChangeRequest>> GetAllByRequestedToEmailAsync(string requestedToEmail, int? status = null);
    }
}

