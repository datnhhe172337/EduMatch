using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;


namespace EduMatch.BusinessLogicLayer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IMapper _mapper;
        //private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationPusher _notificationPusher;
        public NotificationService(
            INotificationRepository repo,
            IMapper mapper,
            INotificationPusher notificationPusher)
            //IHubContext<NotificationHub> hubContext)
        {
            _repo = repo;
            _mapper = mapper;
            _notificationPusher = notificationPusher;
            //_hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(string userEmail, string message, string linkUrl = null)
        {
            var notification = new Notification
            {
                UserEmail = userEmail,
                Message = message,
                LinkUrl = linkUrl,
                IsRead = false,
                CreatedAt = GetVietnamNow()
            };

            var savedNotification = await _repo.CreateAsync(notification);

            var notificationDto = _mapper.Map<NotificationDto>(savedNotification);

            await _notificationPusher.PushNotificationToUserAsync(userEmail, notificationDto);
            //await _hubContext.Clients
            //    .User(userEmail)
            //    .SendAsync("ReceiveNotification", notificationDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(string userEmail, int page, int pageSize)
        {
            var notifications = await _repo.GetByUserAsync(userEmail, page, pageSize);
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userEmail)
        {
            return await _repo.MarkAsReadAsync(notificationId, userEmail);
        }

        public async Task<bool> MarkAllAsReadAsync(string userEmail)
        {
            var rowsAffected = await _repo.MarkAllAsReadAsync(userEmail);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userEmail)
        {
            return await _repo.DeleteAsync(notificationId, userEmail);
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userEmail)
        {
            return await _repo.GetUnreadCountAsync(userEmail);
        }

        private static DateTime GetVietnamNow()
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.UtcNow.AddHours(7);
            }
            catch (InvalidTimeZoneException)
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }
    }
}
