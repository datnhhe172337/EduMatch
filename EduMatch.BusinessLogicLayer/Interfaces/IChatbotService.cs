using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IChatbotService
    {
        Task<int> CreateSessionAsync(string? userEmail);
        Task AddMessageAsync(int sessionId, string role, string message);
        Task<List<ChatbotMessage>> GetLastMessagesAsync(int sessionId, int count);

        Task<List<ChatSession>> GetListSessionsByUserEmail(string userEmail);
        Task<List<ChatbotMessage>> GetMessagesHistoryAsync(int sessionId);
        Task<bool> DeleteSessionAsync(int sessionId);
        bool IsTutorQuery(string message);
    }
}
