using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EduMatch.PresentationLayer
{
    public class NotificationPusher : INotificationPusher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationPusher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushNotificationToUserAsync(string userEmail, NotificationDto notification)
        {
            await _hubContext.Clients
                .User(userEmail)
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
