using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;

        public ChatService(IChatRepository repo)
        {
            _repo = repo;
        }

        public async Task<ChatRoom> GetOrCreateChatRoomAsync(string userEmail, int tutorId)
        {
            if (string.IsNullOrEmpty(userEmail))
                throw new ArgumentNullException(nameof(userEmail));

            var room = await _repo.GetChatRoomAsync(userEmail, tutorId);
            if (room == null)
            {
                room = new ChatRoom
                {
                    UserEmail = userEmail,
                    TutorId = tutorId,
                    CreatedAt = DateTime.UtcNow
                };
                await _repo.CreateChatRoomAsync(room);
            }
            return room;
        }

        public async Task<List<ChatRoom>> GetUserChatRoomsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));
            return await _repo.GetUserChatRoomsAsync(email);
        }

        public async Task<ChatMessage> SendMessageAsync(int chatRoomId, string senderEmail, string receiverEmail, string message)
        {
            if (string.IsNullOrEmpty(senderEmail))
                throw new ArgumentNullException(nameof(senderEmail));
            if (string.IsNullOrEmpty(receiverEmail))
                throw new ArgumentNullException(nameof(receiverEmail));
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));


            var msg = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderEmail = senderEmail,
                ReceiverEmail = receiverEmail,
                MessageText = message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            return await _repo.AddMessageAsync(msg);
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync(int chatRoomId)
        {
            return await _repo.GetMessagesByRoomAsync(chatRoomId);
        }

        public async Task MarkAsReadAsync(int chatRoomId, string receiverEmail)
        {
            if (string.IsNullOrEmpty(receiverEmail))
                throw new ArgumentNullException(nameof(receiverEmail));
            await _repo.MarkMessagesAsReadAsync(chatRoomId, receiverEmail);
        }
    }
}
