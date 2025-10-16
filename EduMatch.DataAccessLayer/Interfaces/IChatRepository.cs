using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IChatRepository
    {
        // ChatRoom operations
        Task<ChatRoom?> GetChatRoomAsync(string userEmail, int tutorId);
        Task<ChatRoom> CreateChatRoomAsync(ChatRoom room);
        Task<List<ChatRoom>> GetUserChatRoomsAsync(string email);

        // ChatMessage operations
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesByRoomAsync(int chatRoomId);
        Task MarkMessagesAsReadAsync(int chatRoomId, string receiverEmail);

        Task SaveChangesAsync();
    }
}
