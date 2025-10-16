using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("rooms/{email}")]
        public async Task<IActionResult> GetChatRooms(string email)
        {
            var rooms = await _chatService.GetUserChatRoomsAsync(email);
            return Ok(rooms);
        }

        [HttpGet("messages/{roomId}")]
        public async Task<IActionResult> GetMessages(int roomId)
        {
            var messages = await _chatService.GetChatMessagesAsync(roomId);
            return Ok(messages);
        }
    }
}
