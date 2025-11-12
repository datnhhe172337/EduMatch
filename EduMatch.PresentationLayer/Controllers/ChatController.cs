// ChatController.cs
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Get all chat rooms for a specific user.
        /// </summary>
        [HttpGet("rooms/{email}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatRoom>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetChatRooms(string email)
        {
            try
            {
                var rooms = await _chatService.GetUserChatRoomsAsync(email);

                if (rooms == null || !rooms.Any())
                    return Ok(ApiResponse<string>.Ok("This user does not have any chat rooms yet."));

                return Ok(ApiResponse<IEnumerable<ChatRoom>>.Ok(rooms));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to load chat rooms.", ex.Message));
            }
        }

        /// <summary>
        /// Get all messages in a chat room by ID.
        /// </summary>
        [HttpGet("messages/{roomId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatMessage>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMessages(int roomId)
        {
            try
            {
                var messages = await _chatService.GetChatMessagesAsync(roomId);

                if (messages == null || !messages.Any())
                    return Ok(ApiResponse<string>.Ok("This chat room does not have any messages yet."));

                return Ok(ApiResponse<IEnumerable<ChatMessage>>.Ok(messages));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to load chat messages.", ex.Message));
            }
        }
        /// <summary>
        /// Gets an existing chat room or creates a new one.
        /// </summary>
        [HttpPost("room")]
        [ProducesResponseType(typeof(ApiResponse<ChatRoom>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrCreateRoom([FromBody] CreateRoomRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserEmail))
                    return BadRequest(ApiResponse<string>.Fail("User email is required."));

                var room = await _chatService.GetOrCreateChatRoomAsync(request.UserEmail, request.TutorId);
                return Ok(ApiResponse<ChatRoom>.Ok(room));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to create or get chat room.", ex.Message));
            }
        }
        public class CreateRoomRequest
        {
            public string UserEmail { get; set; }
            public int TutorId { get; set; }
        }
    }
}
