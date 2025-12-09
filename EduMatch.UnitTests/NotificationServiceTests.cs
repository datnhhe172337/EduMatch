using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Moq;
using Xunit;

namespace EduMatch.UnitTests
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _repo = new();
        private readonly Mock<INotificationPusher> _pusher = new();
        private readonly IMapper _mapper;

        public NotificationServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = new Mapper(config);
        }

        private NotificationService CreateService() => new(_repo.Object, _mapper, _pusher.Object);

        #region CreateNotificationAsync
        [Fact]
        public async Task CreateNotificationAsync_SavesNotificationAndPushesDto()
        {
            var userEmail = "user@test.com";
            var message = "hello";
            var link = "/link";
            var savedNotification = new Notification
            {
                Id = 5,
                UserEmail = userEmail,
                Message = message,
                LinkUrl = link,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _repo.Setup(r => r.CreateAsync(It.IsAny<Notification>())).ReturnsAsync(savedNotification);

            var service = CreateService();

            await service.CreateNotificationAsync(userEmail, message, link);

            _repo.Verify(r => r.CreateAsync(It.Is<Notification>(n =>
                n.UserEmail == userEmail &&
                n.Message == message &&
                n.LinkUrl == link)), Times.Once);

            _pusher.Verify(p => p.PushNotificationToUserAsync(userEmail, It.Is<NotificationDto>(dto =>
                dto.Id == savedNotification.Id &&
                dto.Message == message &&
                dto.LinkUrl == link &&
                dto.CreatedAt == savedNotification.CreatedAt &&
                dto.IsRead == savedNotification.IsRead)), Times.Once);
        }

        [Fact]
        public async Task CreateNotificationAsync_WhenRepositoryThrows_PropagatesException()
        {
            var userEmail = "user@test.com";
            var message = "hello";
            _repo.Setup(r => r.CreateAsync(It.IsAny<Notification>())).ThrowsAsync(new InvalidOperationException("repo fail"));

            var service = CreateService();
 
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateNotificationAsync(userEmail, message, null));

            _pusher.Verify(p => p.PushNotificationToUserAsync(It.IsAny<string>(), It.IsAny<NotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateNotificationAsync_WhenPusherThrows_PropagatesException()
        {
            var userEmail = "user@test.com";
            var message = "hello";
            var savedNotification = new Notification
            {
                Id = 9,
                UserEmail = userEmail,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _repo.Setup(r => r.CreateAsync(It.IsAny<Notification>())).ReturnsAsync(savedNotification);
            _pusher.Setup(p => p.PushNotificationToUserAsync(userEmail, It.IsAny<NotificationDto>()))
                .ThrowsAsync(new ApplicationException("push fail"));

            var service = CreateService();

            await Assert.ThrowsAsync<ApplicationException>(() =>
                service.CreateNotificationAsync(userEmail, message, null));

            _repo.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Once);
            _pusher.Verify(p => p.PushNotificationToUserAsync(userEmail, It.IsAny<NotificationDto>()), Times.Once);
        }
        #endregion

        #region GetNotificationsForUserAsync
        [Fact]
        public async Task GetNotificationsForUserAsync_MapsEntitiesToDtos()
        {
            var userEmail = "user@test.com";
            var notifications = new List<Notification>
            {
                new Notification { Id = 1, UserEmail = userEmail, Message = "m1", CreatedAt = DateTime.UtcNow, IsRead = false, LinkUrl = "/l1" },
                new Notification { Id = 2, UserEmail = userEmail, Message = "m2", CreatedAt = DateTime.UtcNow.AddMinutes(-5), IsRead = true, LinkUrl = "/l2" }
            };

            _repo.Setup(r => r.GetByUserAsync(userEmail, 1, 10)).ReturnsAsync(notifications);

            var service = CreateService();

            var result = await service.GetNotificationsForUserAsync(userEmail, 1, 10);

            var list = result.ToList();
            Assert.Equal(notifications.Count, list.Count);
            Assert.Equal(notifications[0].Id, list[0].Id);
            Assert.Equal(notifications[1].Message, list[1].Message);
            Assert.Equal(notifications[1].LinkUrl, list[1].LinkUrl);
        }

        [Fact]
        public async Task GetNotificationsForUserAsync_WhenRepositoryThrows_PropagatesException()
        {
            var userEmail = "user@test.com";
            _repo.Setup(r => r.GetByUserAsync(userEmail, 2, 5)).ThrowsAsync(new TimeoutException("db timeout"));

            var service = CreateService();

            await Assert.ThrowsAsync<TimeoutException>(() =>
                service.GetNotificationsForUserAsync(userEmail, 2, 5));
        }
        #endregion

        #region MarkAsReadAsync
        [Fact]
        public async Task MarkAsReadAsync_ForwardsToRepository()
        {
            var notificationId = 10;
            var userEmail = "user@test.com";
            _repo.Setup(r => r.MarkAsReadAsync(notificationId, userEmail)).ReturnsAsync(true);

            var service = CreateService();
            var result = await service.MarkAsReadAsync(notificationId, userEmail);

            Assert.True(result);
            _repo.Verify(r => r.MarkAsReadAsync(notificationId, userEmail), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            var notificationId = 11;
            var userEmail = "user@test.com";
            _repo.Setup(r => r.MarkAsReadAsync(notificationId, userEmail)).ReturnsAsync(false);

            var service = CreateService();
            var result = await service.MarkAsReadAsync(notificationId, userEmail);

            Assert.False(result);
            _repo.Verify(r => r.MarkAsReadAsync(notificationId, userEmail), Times.Once);
        }
        #endregion

        #region MarkAllAsReadAsync
        [Theory]
        [InlineData(3, true)]
        [InlineData(0, false)]
        public async Task MarkAllAsReadAsync_ReturnsTrueOnlyWhenRowsAffected(int rows, bool expected)
        {
            var userEmail = "user@test.com";
            _repo.Setup(r => r.MarkAllAsReadAsync(userEmail)).ReturnsAsync(rows);

            var service = CreateService();
            var result = await service.MarkAllAsReadAsync(userEmail);

            Assert.Equal(expected, result);
            _repo.Verify(r => r.MarkAllAsReadAsync(userEmail), Times.Once);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_WhenRepositoryThrows_PropagatesException()
        {
            var userEmail = "user@test.com";
            _repo.Setup(r => r.MarkAllAsReadAsync(userEmail)).ThrowsAsync(new Exception("db fail"));

            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.MarkAllAsReadAsync(userEmail));
        }
        #endregion

        #region DeleteNotificationAsync
        [Fact]
        public async Task DeleteNotificationAsync_ForwardsResult()
        {
            var notificationId = 7;
            var userEmail = "user@test.com";
            _repo.Setup(r => r.DeleteAsync(notificationId, userEmail)).ReturnsAsync(true);

            var service = CreateService();
            var result = await service.DeleteNotificationAsync(notificationId, userEmail);

            Assert.True(result);
            _repo.Verify(r => r.DeleteAsync(notificationId, userEmail), Times.Once);
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            var notificationId = 8;
            var userEmail = "user@test.com";
            _repo.Setup(r => r.DeleteAsync(notificationId, userEmail)).ReturnsAsync(false);

            var service = CreateService();
            var result = await service.DeleteNotificationAsync(notificationId, userEmail);

            Assert.False(result);
            _repo.Verify(r => r.DeleteAsync(notificationId, userEmail), Times.Once);
        }
        #endregion

        #region GetUnreadNotificationCountAsync
        [Fact]
        public async Task GetUnreadNotificationCountAsync_ReturnsCountFromRepository()
        {
            var userEmail = "user@test.com";
            _repo.Setup(r => r.GetUnreadCountAsync(userEmail)).ReturnsAsync(4);

            var service = CreateService();
            var count = await service.GetUnreadNotificationCountAsync(userEmail);

            Assert.Equal(4, count);
            _repo.Verify(r => r.GetUnreadCountAsync(userEmail), Times.Once);
        }

        [Fact]
        public async Task GetUnreadNotificationCountAsync_WhenRepositoryThrows_PropagatesException()
        {
            var userEmail = "user@test.com";
            _repo.Setup(r => r.GetUnreadCountAsync(userEmail)).ThrowsAsync(new Exception("db fail"));

            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(() =>
                service.GetUnreadNotificationCountAsync(userEmail));
        }
        #endregion
    }
}
