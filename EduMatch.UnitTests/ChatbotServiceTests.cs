using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.UnitTests
{
    public class ChatbotServiceTests
    {
        private readonly Mock<IChatbotRepository> _repoMock;
        private readonly ChatbotService _service;

        public ChatbotServiceTests()
        {
            _repoMock = new Mock<IChatbotRepository>();
            _service = new ChatbotService(_repoMock.Object);
        }

        [Fact]
        public async Task CreateSession_ShouldReturnSessionId_WithValidEmail_UnitTest()
        {
            // Arrange
            var userEmail = "user@example.com";
            var session = new ChatSession { Id = 1, UserEmail = userEmail };

            _repoMock.Setup(r => r.CreateSessionAsync(It.IsAny<ChatSession>()))
                     .Callback<ChatSession>(s => s.Id = 1)
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateSessionAsync(userEmail);

            // Assert
            Assert.Equal(1, result);
            _repoMock.Verify(r => r.CreateSessionAsync(It.Is<ChatSession>(s => s.UserEmail == userEmail)), Times.Once);
        }

        [Fact]
        public async Task CreateSession_ShouldReturnSessionId_WithNullEmail_UnitTest()
        {
            // Arrange
            var session = new ChatSession { Id = 2, UserEmail = null };

            _repoMock.Setup(r => r.CreateSessionAsync(It.IsAny<ChatSession>()))
                     .Callback<ChatSession>(s => s.Id = 2)
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateSessionAsync(null);

            // Assert
            Assert.Equal(2, result);
            _repoMock.Verify(r => r.CreateSessionAsync(It.Is<ChatSession>(s => s.UserEmail == null)), Times.Once);
        }

        [Fact]
        public async Task CreateSession_Throws_WhenRepositoryFails_UnitTest()
        {
            // Arrange
            _repoMock.Setup(r => r.CreateSessionAsync(It.IsAny<ChatSession>()))
                     .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateSessionAsync("user@example.com"));
        }


        [Fact]
        public async Task AddMessage_ShouldCallRepository_WithValidData_UnitTest()
        {
            // Arrange
            int sessionId = 1;
            string role = "user";
            string message = "Hello AI";

            _repoMock.Setup(r => r.AddMessageAsync(It.IsAny<ChatbotMessage>()))
                     .Returns(Task.CompletedTask);

            // Act
            await _service.AddMessageAsync(sessionId, role, message);

            // Assert
            _repoMock.Verify(r => r.AddMessageAsync(It.Is<ChatbotMessage>(
                m => m.SessionId == sessionId &&
                     m.Role == role &&
                     m.Message == message
            )), Times.Once);
        }

        [Fact]
        public async Task AddMessage_ShouldCallRepository_WithEmptyRole_UnitTest()
        {
            // Arrange
            int sessionId = 1;
            string role = "";
            string message = "Hello";

            _repoMock.Setup(r => r.AddMessageAsync(It.IsAny<ChatbotMessage>()))
                     .Returns(Task.CompletedTask);

            // Act
            await _service.AddMessageAsync(sessionId, role, message);

            // Assert
            _repoMock.Verify(r => r.AddMessageAsync(It.Is<ChatbotMessage>(
                m => m.SessionId == sessionId &&
                     m.Role == role &&
                     m.Message == message
            )), Times.Once);
        }

        [Fact]
        public async Task AddMessage_ShouldCallRepository_WithEmptyMessage_UnitTest()
        {
            // Arrange
            int sessionId = 1;
            string role = "user";
            string message = "";

            _repoMock.Setup(r => r.AddMessageAsync(It.IsAny<ChatbotMessage>()))
                     .Returns(Task.CompletedTask);

            // Act
            await _service.AddMessageAsync(sessionId, role, message);

            // Assert
            _repoMock.Verify(r => r.AddMessageAsync(It.Is<ChatbotMessage>(
                m => m.SessionId == sessionId &&
                     m.Role == role &&
                     m.Message == message
            )), Times.Once);
        }

        [Fact]
        public async Task AddMessage_Throws_WhenRepositoryFails_UnitTest()
        {
            // Arrange
            _repoMock.Setup(r => r.AddMessageAsync(It.IsAny<ChatbotMessage>()))
                     .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AddMessageAsync(1, "user", "Hi"));
        }


        [Fact]
        public async Task DeleteSession_ReturnsTrue_WhenSessionExists_UnitTest()
        {
            // Arrange
            int sessionId = 1;
            var session = new ChatSession { Id = sessionId };
            _repoMock.Setup(r => r.GetSessionByIdAsync(sessionId))
                     .ReturnsAsync(session);
            _repoMock.Setup(r => r.RemoveRangeMessageBelongSession(sessionId))
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteSessionAsync(session))
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteSessionAsync(sessionId);

            // Assert
            Assert.True(result);
            _repoMock.Verify(r => r.RemoveRangeMessageBelongSession(sessionId), Times.Once);
            _repoMock.Verify(r => r.DeleteSessionAsync(session), Times.Once);
        }

        [Fact]
        public async Task DeleteSession_ReturnsFalse_WhenSessionNotFound_UnitTest()
        {
            // Arrange
            int sessionId = 1;
            _repoMock.Setup(r => r.GetSessionByIdAsync(sessionId))
                     .ReturnsAsync((ChatSession)null);

            // Act
            var result = await _service.DeleteSessionAsync(sessionId);

            // Assert
            Assert.False(result);
            _repoMock.Verify(r => r.RemoveRangeMessageBelongSession(It.IsAny<int>()), Times.Never);
            _repoMock.Verify(r => r.DeleteSessionAsync(It.IsAny<ChatSession>()), Times.Never);
        }

        [Fact]
        public async Task DeleteSession_Throws_WhenRepositoryFails_UnitTest()
        {
            // Arrange
            int sessionId = 1;
            var session = new ChatSession { Id = sessionId };
            _repoMock.Setup(r => r.GetSessionByIdAsync(sessionId))
                     .ReturnsAsync(session);
            _repoMock.Setup(r => r.RemoveRangeMessageBelongSession(sessionId))
                     .ThrowsAsync(new Exception("DB failure"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteSessionAsync(sessionId));
            Assert.Equal("DB failure", ex.Message);
        }
    }
}
