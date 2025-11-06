using AutoMapper;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.UnitTests.FakeData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace EduMatch.UnitTests
{
    public class UserUnitTests
    {

        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IRefreshTokenRepositoy> _refreshRepoMock;
        private readonly Mock<IGoogleAuthService> _googleAuthMock;
        private readonly Mock<IUserProfileRepository> _profileRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtOptionsMock;

        private readonly UserService _userService;

        public UserUnitTests()
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


            _userService = new UserService(
                _userRepoMock.Object,
                _mapperMock.Object,
                _jwtOptionsMock.Object,
                new FakeEmailService(),
                _refreshRepoMock.Object,
                _googleAuthMock.Object,
                _profileRepoMock.Object
            );
        }

        [Fact]
        public async Task Register_ReturnFalse_WhenEmailAlreadyExists_UnitTest()
        {
            // Arrange
            var email = "vietnq021103@gmail.com";
            _userRepoMock.Setup(r => r.IsEmailAvailableAsync(email))
                         .ReturnsAsync(true);

            // Act
            var result = await _userService.RegisterAsync("Nguyễn Quốc Việt", email, "123", "https://localhost");

            // Assert
            Assert.False(result);

            _userRepoMock.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_CreateUser_WhenEmailIsNew_UnitTest()
        {
            // Arrange
            var email = "newuser@example.com";
            _userRepoMock.Setup(r => r.IsEmailAvailableAsync(email))
                         .ReturnsAsync(false);

            _userRepoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterAsync("New User", email, "123", "https://localhost");

            // Assert
            Assert.True(result);

            _userRepoMock.Verify(r => r.CreateUserAsync(It.Is<User>(u => u.Email == email)), Times.Once);
        }
    }
}