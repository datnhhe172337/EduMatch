using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly IChatbotRepository _repo;

        public ChatbotService(IChatbotRepository repo)
        {
            _repo = repo;
        }

        public async Task AddMessageAsync(int sessionId, string role, string message)
        {
            var msg = new ChatbotMessage
            {
                SessionId = sessionId,
                Role = role,
                Message = message
            };

            await _repo.AddMessageAsync(msg);
        }

        public async Task<int> CreateSessionAsync(string? userEmail)
        {
            var session = new ChatSession
            {
                UserEmail = userEmail
            };

            await _repo.CreateSessionAsync(session);

            return session.Id;
        }

        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            var session = await _repo.GetSessionByIdAsync(sessionId);
            if (session == null) return false;

            try
            {
                await _repo.RemoveRangeMessageBelongSession(sessionId);
                await _repo.DeleteSessionAsync(session);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ChatbotMessage>> GetLastMessagesAsync(int sessionId, int count)
        {
            return await _repo.GetLastMessagesAsync(sessionId, count);
        }

        public async Task<List<ChatSession>> GetListSessionsByUserEmail(string userEmail)
        {
            return await _repo.GetListSessions(userEmail);
            
        }

        public async Task<List<ChatbotMessage>> GetMessagesHistoryAsync(int sessionId)
        {
            return await _repo.GetMessagesHistoryAsync(sessionId);
        }

        public bool IsTutorQuery(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;

            string lower = message.ToLower();
            string[] keywords = { "gia sư", "dạy", "môn", "lớp", "học sinh", "học phí", "khu vực", "học online", "offline", "trực tiếp", "trực tuyến", "giá", "phí", "kèm", "gv", "day", "lop", "thầy", "cô", "toán", "lý", "hóa", "Văn", "Anh", "tutor"};

            return keywords.Any(k => lower.Contains(k));
        }

    }
}
