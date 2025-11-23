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
    public class ChatbotRepository : IChatbotRepository
    {
        private readonly EduMatchContext _context;

        public ChatbotRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync(ChatbotMessage message)
        {
            await _context.ChatbotMessages.AddAsync(message);
            _context.SaveChanges();
        }

        public async Task CreateSessionAsync(ChatSession session)
        {
            await _context.ChatSessions.AddAsync(session);
            _context.SaveChanges();
        }

        public async Task DeleteSessionAsync(ChatSession session)
        {
           _context.ChatSessions.Remove(session);
           await _context.SaveChangesAsync();
        }

        public async Task<List<ChatbotMessage>> GetLastMessagesAsync(int sessionId, int count)
        {
            var messages = await _context.ChatbotMessages
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.Id)   // lấy từ mới nhất
                .Take(count)
                .ToListAsync();

            // Đưa về đúng thứ tự: cũ → mới
            return messages.OrderBy(x => x.Id).ToList();
        }

        public async Task<List<ChatSession>> GetListSessions(string userEmail)
        {
            return await _context.ChatSessions
                .Include(s => s.ChatbotMessages)
                .Where(s => s.UserEmail == userEmail)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        }

        public async Task<List<ChatbotMessage>> GetMessagesHistoryAsync(int sessionId)
        {
            return await _context.ChatbotMessages
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        }

        public async Task<ChatSession> GetSessionByIdAsync(int sessionId)
        {
            return await _context.ChatSessions
            .Where(s => s.Id == sessionId)
            .FirstOrDefaultAsync();
            
        }

        public async Task RemoveRangeMessageBelongSession(int sessionId)
        {
            var messages = _context.ChatbotMessages
                .Where(m => m.SessionId == sessionId);

            _context.ChatbotMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }
    }
}
