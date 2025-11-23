using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IChatbotRepository
    {
        Task CreateSessionAsync(ChatSession session);

        Task AddMessageAsync(ChatbotMessage message);

        Task<List<ChatbotMessage>> GetLastMessagesAsync(int sessionId, int count);

        Task<List<ChatbotMessage>> GetMessagesHistoryAsync(int sessionId);

        Task<List<ChatSession>> GetListSessions(string userEmail);

        Task DeleteSessionAsync(ChatSession session);

        Task<ChatSession> GetSessionByIdAsync(int sessionId);

        Task RemoveRangeMessageBelongSession(int sessionId);
    }
}
