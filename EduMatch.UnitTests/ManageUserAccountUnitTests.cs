using AutoMapper;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.UnitTests.FakeData;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EduMatch.UnitTests
{
    public class ManageUserAccountUnitTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IRefreshTokenRepositoy> _refreshRepoMock;
        private readonly Mock<IGoogleAuthService> _googleAuthMock;
        private readonly Mock<IUserProfileRepository> _profileRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtOptionsMock;
        private readonly FakeEmailService _fakeEmailService;

        private readonly UserService _userService;

        public ManageUserAccountUnitTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _refreshRepoMock = new Mock<IRefreshTokenRepositoy>();
            _googleAuthMock = new Mock<IGoogleAuthService>();
            _profileRepoMock = new Mock<IUserProfileRepository>();
            _mapperMock = new Mock<IMapper>();
            _jwtOptionsMock = new Mock<IOptions<JwtSettings>>();

            _jwtOptionsMock.Setup(x => x.Value).Returns(new JwtSettings
            {
                Secret = "SuperSecureKey_123456789_TestSecret",
                Issuer = "FakeIssuer",
                Audience = "FakeAudience",
                AccessTokenMinutes = 10,
                RefreshTokenDays = 30
            });

            _fakeEmailService = new FakeEmailService();

            _userService = new UserService(
                _userRepoMock.Object,
                _mapperMock.Object,
                _jwtOptionsMock.Object,
                _fakeEmailService,
                _refreshRepoMock.Object,
                _googleAuthMock.Object,
                _profileRepoMock.Object
            );
        }

        // ==========================================================
        // TEST GetUserByRoleAsync
        // ==========================================================
        [Fact]
        public async Task GetUserByRoleAsync_Learner_ReturnsMappedList()
        {
            var learners = new List<User>
            {
                new User
                {
                    Email = "learner@example.com",
                    UserName = "learner",
                    Role = new Role { RoleName = "Learner" },
                    Phone = "111",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            };
            _userRepoMock.Setup(r => r.GetLearnerAsync()).ReturnsAsync(learners);

            var result = await _userService.GetUserByRoleAsync(1);

            Assert.Single(result);
            var dto = result.First();
            Assert.Equal("learner@example.com", dto.Email);
            Assert.Equal("Learner", dto.RoleName);
        }

        [Fact]
        public async Task GetUserByRoleAsync_Tutor_ReturnsMappedList()
        {
            var tutors = new List<User>
            {
                new User
                {
                    Email = "tutor@example.com",
                    UserName = "tutor",
                    Role = new Role { RoleName = "Tutor" },
                    Phone = "222",
                    IsActive = true,
                    TutorProfile = new TutorProfile { CreatedAt = new DateTime(2024, 5, 5) },
                    CreatedAt = new DateTime(2023, 1, 1)
                }
            };
            _userRepoMock.Setup(r => r.GetTutorAsync()).ReturnsAsync(tutors);

            var result = await _userService.GetUserByRoleAsync(2);

            Assert.Single(result);
            var dto = result.First();
            Assert.Equal("tutor@example.com", dto.Email);
            Assert.Equal(new DateTime(2024, 5, 5), dto.CreateAt);
        }

        [Fact]
        public async Task GetUserByRoleAsync_Admin_ReturnsMappedList()
        {
            var admins = new List<User>
            {
                new User
                {
                    Email = "admin@example.com",
                    UserName = "admin",
                    Role = new Role { RoleName = "Admin" },
                    Phone = "333",
                    IsActive = true,
                    CreatedAt = new DateTime(2023, 1, 1)
                }
            };
            _userRepoMock.Setup(r => r.GetAdminAsync()).ReturnsAsync(admins);

            var result = await _userService.GetUserByRoleAsync(3);

            Assert.Single(result);
            Assert.Equal("Admin", result.First().RoleName);
        }

        [Fact]
        public async Task GetUserByRoleAsync_InvalidRole_ReturnsEmpty()
        {
            var result = await _userService.GetUserByRoleAsync(99);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserByRoleAsync_Fail_RepoReturnsNull()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetLearnerAsync())
                         .ReturnsAsync((List<User>)null);

            // Act + Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                async () => await _userService.GetUserByRoleAsync(1)
            );
        }

        [Fact]
        public async Task GetUserByRoleAsync_Fail_MissingRoleName()
        {
            // Arrange
            var list = new List<User> { new User { Email = "a@b.com", Role = null } };
            _userRepoMock.Setup(r => r.GetTutorAsync()).ReturnsAsync(list);

            // Act + Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                async () => await _userService.GetUserByRoleAsync(2)
            );
        }

        [Fact]
        public async Task GetUserByRoleAsync_Fail_TutorProfileNull_ThrowsException()
        {
            // Arrange
            var tutors = new List<User>
            {
            new User
                {
                    Email = "tutor@example.com",
                    Role = new Role { RoleName = "Tutor" },
                    TutorProfile = null // Lỗi: null nhưng service vẫn truy cập CreatedAt
                }
            };
            _userRepoMock.Setup(r => r.GetTutorAsync()).ReturnsAsync(tutors);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _userService.GetUserByRoleAsync(2));
        }

        // ==========================================================
        // TEST ActivateUserAsync & DeactivateUserAsync
        // ==========================================================
        [Fact]
        public async Task DeactivateUserAsync_ReturnsTrue_WhenRepoSucceeds()
        {
            _userRepoMock.Setup(r => r.UpdateUserStatusAsync("test@example.com", false))
                         .ReturnsAsync(true);

            var result = await _userService.DeactivateUserAsync("test@example.com");

            Assert.True(result);
            _userRepoMock.Verify(r => r.UpdateUserStatusAsync("test@example.com", false), Times.Once);
        }

        [Fact]
        public async Task ActivateUserAsync_ReturnsTrue_WhenRepoSucceeds()
        {
            _userRepoMock.Setup(r => r.UpdateUserStatusAsync("test@example.com", true))
                         .ReturnsAsync(true);

            var result = await _userService.ActivateUserAsync("test@example.com");

            Assert.True(result);
            _userRepoMock.Verify(r => r.UpdateUserStatusAsync("test@example.com", true), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_ReturnsFalse_WhenRepoFails()
        {
            // Arrange
            _userRepoMock.Setup(r => r.UpdateUserStatusAsync("fail@example.com", false))
                         .ReturnsAsync(false); // Giả lập repo báo thất bại

            // Act
            var result = await _userService.DeactivateUserAsync("fail@example.com");

            // Assert
            Assert.False(result); // Kỳ vọng service cũng trả về false
        }

        [Fact]
        public async Task ActivateUserAsync_ReturnsFalse_WhenRepoFails()
        {
            // Arrange
            _userRepoMock.Setup(r => r.UpdateUserStatusAsync("fail@example.com", true))
                         .ReturnsAsync(false);

            // Act
            var result = await _userService.ActivateUserAsync("fail@example.com");

            // Assert
            Assert.False(result);
        }

        // ==========================================================
        // TEST CreateAdminAccAsync
        // ==========================================================
        [Fact]
        public async Task CreateAdminAccAsync_CreatesAdminAndSendsEmail()
        {
            _userRepoMock.Setup(r => r.GetUserByEmailAsync("newadmin@example.com"))
                         .ReturnsAsync((User)null);
            _userRepoMock.Setup(r => r.CreateAdminAccAsync(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            var admin = await _userService.CreateAdminAccAsync("newadmin@example.com", "password123");

            Assert.Equal("newadmin@example.com", admin.Email);
            Assert.Equal(3, admin.RoleId);
        }

        [Fact]
        public async Task CreateAdminAccAsync_Throws_WhenEmailExists()
        {
            _userRepoMock.Setup(r => r.GetUserByEmailAsync("exist@example.com"))
                         .ReturnsAsync(new User { Email = "exist@example.com" });

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _userService.CreateAdminAccAsync("exist@example.com", "pass123")
            );
        } 

        [Fact]
        public async Task CreateAdminAccAsync_Fail_InvalidEmailFormat_ThrowsException()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetUserByEmailAsync("invalidemail"))
                         .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<IndexOutOfRangeException>(() =>
                _userService.CreateAdminAccAsync("invalidemail", "pass123"));
        }
    }
}
