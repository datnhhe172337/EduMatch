using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.SignalR;

namespace EduMatch.PresentationLayer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;

        public ChatHub(ChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task SendMessage(int chatRoomId, string senderEmail, string receiverEmail, string message)
        {
            var msg = await _chatService.SendMessageAsync(chatRoomId, senderEmail, receiverEmail, message);

            await Clients.User(receiverEmail).SendAsync("ReceiveMessage", msg);
            await Clients.User(senderEmail).SendAsync("ReceiveMessage", msg);
        }

        public async Task MarkMessagesAsRead(int chatRoomId, string receiverEmail)
        {
            await _chatService.MarkAsReadAsync(chatRoomId, receiverEmail);
        }
    }
}
