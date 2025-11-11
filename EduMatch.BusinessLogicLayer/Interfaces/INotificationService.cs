using EduMatch.BusinessLogicLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface INotificationService
    {
        // Internal method
        Task CreateNotificationAsync(string userEmail, string message, string linkUrl = null);

        Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(string userEmail, int page, int pageSize);
        Task<bool> MarkAsReadAsync(int notificationId, string userEmail);
        Task<bool> MarkAllAsReadAsync(string userEmail);
        Task<bool> DeleteNotificationAsync(int notificationId, string userEmail);
        Task<int> GetUnreadNotificationCountAsync(string userEmail);
    }
}
