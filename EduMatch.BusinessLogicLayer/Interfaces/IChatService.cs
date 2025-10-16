using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IChatService
    {
        Task<ChatRoom> GetOrCreateChatRoomAsync(string userEmail, int tutorId);
        Task<List<ChatRoom>> GetUserChatRoomsAsync(string email);
        Task<ChatMessage> SendMessageAsync(int chatRoomId, string senderEmail, string receiverEmail, string message);
        Task<List<ChatMessage>> GetChatMessagesAsync(int chatRoomId);
        Task MarkAsReadAsync(int chatRoomId, string receiverEmail);
    }
}
