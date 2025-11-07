using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatRepository> _mockRepo;
        private readonly IChatService _service;

        public ChatServiceTests()
        {
            _mockRepo = new Mock<IChatRepository>();
            _service = new ChatService(_mockRepo.Object);
        }

        #region GetOrCreateChatRoomAsync Tests

        [Fact]
        public async Task GetOrCreateChatRoomAsync_ShouldReturnExistingRoom_WhenRoomAlreadyExists()
        {
            // Arrange
            var userEmail = "student@example.com";
            var tutorId = 1;
            var existingRoom = new ChatRoom { Id = 101, UserEmail = userEmail, TutorId = tutorId };

            _mockRepo.Setup(r => r.GetChatRoomAsync(userEmail, tutorId)).ReturnsAsync(existingRoom);

            // Act
            var result = await _service.GetOrCreateChatRoomAsync(userEmail, tutorId);

            // Assert
            result.Should().Be(existingRoom);
            _mockRepo.Verify(r => r.CreateChatRoomAsync(It.IsAny<ChatRoom>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateChatRoomAsync_ShouldCreateNewRoom_WhenRoomDoesNotExist()
        {
            // Arrange
            var userEmail = "student@example.com";
            var tutorId = 1;

            _mockRepo.Setup(r => r.GetChatRoomAsync(userEmail, tutorId)).ReturnsAsync((ChatRoom)null);
            _mockRepo.Setup(r => r.CreateChatRoomAsync(It.IsAny<ChatRoom>()))
         .ReturnsAsync((ChatRoom room) => room);

            // Act
            var result = await _service.GetOrCreateChatRoomAsync(userEmail, tutorId);

            // Assert
            result.Should().NotBeNull();
            result.UserEmail.Should().Be(userEmail);
            result.TutorId.Should().Be(tutorId);
            _mockRepo.Verify(r => r.CreateChatRoomAsync(It.Is<ChatRoom>(
                room => room.UserEmail == userEmail && room.TutorId == tutorId
            )), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateChatRoomAsync_ShouldThrowArgumentNullException_WhenEmailIsNull()
        {
            // Act
            Func<Task> act = async () => await _service.GetOrCreateChatRoomAsync(null, 1);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("userEmail");
        }

        #endregion

        #region GetUserChatRoomsAsync Tests

        [Fact]
        public async Task GetUserChatRoomsAsync_ShouldReturnUserRooms_WhenCalled()
        {
            // Arrange
            var email = "student@example.com";
            var fakeRooms = new List<ChatRoom>
            {
                new ChatRoom { Id = 1, UserEmail = email, TutorId = 1 },
                new ChatRoom { Id = 2, UserEmail = email, TutorId = 2 }
            };
            _mockRepo.Setup(r => r.GetUserChatRoomsAsync(email)).ReturnsAsync(fakeRooms);

            // Act
            var result = await _service.GetUserChatRoomsAsync(email);

            // Assert
            result.Should().BeEquivalentTo(fakeRooms);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUserChatRoomsAsync_ShouldReturnEmptyList_WhenNoRoomsExist()
        {
            // Arrange
            var email = "student@example.com";
            var emptyList = new List<ChatRoom>();
            _mockRepo.Setup(r => r.GetUserChatRoomsAsync(email)).ReturnsAsync(emptyList);

            // Act
            var result = await _service.GetUserChatRoomsAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserChatRoomsAsync_ShouldThrowArgumentNullException_WhenEmailIsNull()
        {
            // Act
            Func<Task> act = async () => await _service.GetUserChatRoomsAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("email");
        }

        #endregion

        #region SendMessageAsync Tests

        [Fact]
        public async Task SendMessageAsync_ShouldCreateAndAddMessage_WhenCalled()
        {
            // Arrange
            var chatRoomId = 1;
            var sender = "sender@example.com";
            var receiver = "receiver@example.com";
            var messageText = "Hello!";

            // Setup mock to return the message that was passed to it
            _mockRepo.Setup(r => r.AddMessageAsync(It.IsAny<ChatMessage>()))
                     .ReturnsAsync((ChatMessage msg) => {
                         msg.Id = 99; // Simulate DB assigning an ID
                         return msg;
                     });

            // Act
            var result = await _service.SendMessageAsync(chatRoomId, sender, receiver, messageText);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(99);
            result.ChatRoomId.Should().Be(chatRoomId);
            result.SenderEmail.Should().Be(sender);
            result.ReceiverEmail.Should().Be(receiver);
            result.MessageText.Should().Be(messageText);
            result.IsRead.Should().BeFalse();
            result.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            _mockRepo.Verify(r => r.AddMessageAsync(It.IsAny<ChatMessage>()), Times.Once);
        }

        [Theory]
        [InlineData(null, "receiver", "message", "senderEmail")]
        [InlineData("sender", null, "message", "receiverEmail")]
        [InlineData("sender", "receiver", null, "message")]
        public async Task SendMessageAsync_ShouldThrowArgumentNullException_ForInvalidInputs(
            string sender, string receiver, string message, string paramName)
        {
            // Act
            Func<Task> act = async () => await _service.SendMessageAsync(1, sender, receiver, message);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName(paramName);
        }

        #endregion

        #region GetChatMessagesAsync Tests

        [Fact]
        public async Task GetChatMessagesAsync_ShouldReturnMessages_WhenCalled()
        {
            // Arrange
            var chatRoomId = 1;
            var fakeMessages = new List<ChatMessage>
            {
                new ChatMessage { Id = 1, MessageText = "Hi" },
                new ChatMessage { Id = 2, MessageText = "How are you?" }
            };
            _mockRepo.Setup(r => r.GetMessagesByRoomAsync(chatRoomId)).ReturnsAsync(fakeMessages);

            // Act
            var result = await _service.GetChatMessagesAsync(chatRoomId);

            // Assert
            result.Should().BeEquivalentTo(fakeMessages);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetChatMessagesAsync_ShouldReturnEmptyList_WhenNoMessagesExist()
        {
            // Arrange
            var chatRoomId = 1;
            var emptyList = new List<ChatMessage>();
            _mockRepo.Setup(r => r.GetMessagesByRoomAsync(chatRoomId)).ReturnsAsync(emptyList);

            // Act
            var result = await _service.GetChatMessagesAsync(chatRoomId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region MarkAsReadAsync Tests

        [Fact]
        public async Task MarkAsReadAsync_ShouldCallRepository_WhenCalled()
        {
            // Arrange
            var chatRoomId = 1;
            var receiverEmail = "receiver@example.com";
            _mockRepo.Setup(r => r.MarkMessagesAsReadAsync(chatRoomId, receiverEmail))
                     .Returns(Task.CompletedTask);

            // Act
            await _service.MarkAsReadAsync(chatRoomId, receiverEmail);

            // Assert
            _mockRepo.Verify(r => r.MarkMessagesAsReadAsync(chatRoomId, receiverEmail), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldThrowArgumentNullException_WhenEmailIsNull()
        {
            // Act
            Func<Task> act = async () => await _service.MarkAsReadAsync(1, null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("receiverEmail");
        }

        #endregion
    }

    
}