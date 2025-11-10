using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly EduMatchContext _context;

        public NotificationRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> GetByUserAsync(string userEmail, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserEmail == userEmail)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Notification> GetByIdAsync(int notificationId, string userEmail)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserEmail == userEmail);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userEmail)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserEmail == userEmail && !n.IsRead);

            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> MarkAllAsReadAsync(string userEmail)
        {
            return await _context.Notifications
                .Where(n => n.UserEmail == userEmail && !n.IsRead)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(n => n.IsRead, true)
                     .SetProperty(n => n.ReadAt, DateTime.UtcNow));
        }

        public async Task<bool> DeleteAsync(int notificationId, string userEmail)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserEmail == userEmail);

            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetUnreadCountAsync(string userEmail)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserEmail == userEmail && !n.IsRead);
        }
    }
}
