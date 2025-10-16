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
    public class ChatRepository : IChatRepository
    {
        private readonly EduMatchContext _context;

        public ChatRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<ChatRoom?> GetChatRoomAsync(string userEmail, int tutorId)
        {
            return await _context.ChatRooms
                .Include(r => r.ChatMessages)
                .FirstOrDefaultAsync(r => r.UserEmail == userEmail && r.TutorId == tutorId);
        }

        public async Task<ChatRoom> CreateChatRoomAsync(ChatRoom room)
        {
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<List<ChatRoom>> GetUserChatRoomsAsync(string email)
        {
            return await _context.ChatRooms
                .Include(r => r.Tutor)
                .Include(r => r.UserEmailNavigation)
                .Where(r => r.UserEmail == email || r.Tutor.UserEmailNavigation.Email == email)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetMessagesByRoomAsync(int chatRoomId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(int chatRoomId, string receiverEmail)
        {
            var unread = await _context.ChatMessages
                .Where(m => m.ChatRoomId == chatRoomId && m.ReceiverEmail == receiverEmail && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unread)
                msg.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
