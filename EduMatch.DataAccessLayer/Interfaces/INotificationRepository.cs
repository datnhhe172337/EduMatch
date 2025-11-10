using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<IEnumerable<Notification>> GetByUserAsync(string userEmail, int page, int pageSize);
        Task<Notification> GetByIdAsync(int notificationId, string userEmail);
        Task<bool> MarkAsReadAsync(int notificationId, string userEmail);
        Task<int> MarkAllAsReadAsync(string userEmail);
        Task<bool> DeleteAsync(int notificationId, string userEmail);
        Task<int> GetUnreadCountAsync(string userEmail);
    }
}
