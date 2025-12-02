using AutoMapper;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.UnitTests.FakeData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        // Token invalid 

        [Fact]
        public async Task VerifyEmail_ReturnsFalse_WhenTokenInvalid_UnitTest()
        {
            // Act
            var result = await _userService.VerifyEmailAsync("invalid.token");

            // Assert
            Assert.False(result);
        }


        // Token hợp lệ nhưng không có email claim

        [Fact]
        public async Task VerifyEmail_ReturnsFalse_WhenEmailClaimMissing_UnitTest()
        {
            // Arrange
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptionsMock.Object.Value.Secret);

            var token = tokenHandler.CreateEncodedJwt(
                issuer: _jwtOptionsMock.Object.Value.Issuer,
                audience: null,
                subject: new ClaimsIdentity(), // No claim sub
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(5),
                issuedAt: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            // Act
            var result = await _userService.VerifyEmailAsync(token);

            // Assert
            Assert.False(result);
        }


        //Token hợp lệ nhưng user email không tồn tại trong DB

        [Fact]
        public async Task VerifyEmail_ReturnsFalse_WhenUserNotFound_UnitTest()
        {
            // Arrange
            var token = GenerateValidToken("notfound@example.com");
            _userRepoMock.Setup(r => r.GetUserByEmailAsync("notfound@example.com"))
                         .ReturnsAsync((User)null);

            // Act
            var result = await _userService.VerifyEmailAsync(token);

            // Assert
            Assert.False(result);
        }


        //Token hợp lệ, user tồn tại

        [Fact]
        public async Task VerifyEmail_ReturnsTrue_WhenValidTokenAndUserExists_UnitTest()
        {
            // Arrange
            var token = GenerateValidToken("user@example.com");
            var user = new User { Email = "user@example.com" };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync("user@example.com"))
                         .ReturnsAsync(user);

            _userRepoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            _profileRepoMock.Setup(p => p.CreateUserProfileAsync(It.IsAny<UserProfile>()))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.VerifyEmailAsync(token);

            // Assert
            Assert.True(result);
            _userRepoMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u => (u.IsEmailConfirmed ?? false) && (u.IsActive ?? false))), Times.Once);

            _profileRepoMock.Verify(p => p.CreateUserProfileAsync(It.IsAny<UserProfile>()), Times.Once);
        }



        private string GenerateValidToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptionsMock.Object.Value.Secret);

            var token = tokenHandler.CreateEncodedJwt(
                issuer: _jwtOptionsMock.Object.Value.Issuer,
                audience: null,
                subject: new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, email) }),
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(10),
                issuedAt: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        [Fact]
        public async Task Login_ShouldThrow_WhenEmailNotFound_UnitTest()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetUserByEmailAsync("none@example.com")).ReturnsAsync((User)null);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _userService.LoginAsync("none@example.com", "123")
            );

            Assert.Equal("Invalid email", ex.Message);
        }

        [Fact]
        public async Task Login_ShouldThrow_WhenLoginProviderIsGoogle_UnitTest()
        {
            var user = new User { Email = "a@gmail.com", LoginProvider = "Google" };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _userService.LoginAsync(user.Email, "pass")
            );

            Assert.Equal("Email is logged in with google", ex.Message);
        }

        [Fact]
        public async Task Login_ShouldReturnDto_WhenSuccess_NoExistingToken_UnitTest()
        {
            // Arrange
            var user = new User
            {
                Email = "ok@example.com",
                LoginProvider = "Local",
                IsEmailConfirmed = true,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = new Role { RoleName = "User" }
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _refreshRepoMock.Setup(r => r.ExistingRefreshTokenAsync(It.IsAny<User>())).ReturnsAsync((RefreshToken)null);
            _refreshRepoMock.Setup(r => r.CreateRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.LoginAsync(user.Email, "123456");

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            _refreshRepoMock.Verify(r => r.CreateRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        
        [Fact]
        public async Task Login_ShouldReturnNull_WhenPasswordIncorrect_UnitTest()
        {
            var user = new User
            {
                Email = "user@example.com",
                LoginProvider = "Local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPass"),
                IsEmailConfirmed = true,
                IsActive = true,

            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var result = await _userService.LoginAsync(user.Email, "wrongPass");

            Assert.Null(result);
        }

        
        [Fact]
        public async Task Login_ShouldThrow_WhenEmailNotVerified_UnitTest()
        {
            var user = new User
            {
                Email = "unverified@example.com",
                LoginProvider = "Local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                IsEmailConfirmed = false,
                IsActive = true
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _userService.LoginAsync(user.Email, "123")
            );

            Assert.Equal("Email not verified. Please verify before login!", ex.Message);
        }


        [Fact]
        public async Task Login_ShouldThrow_WhenAccountDeactivated_UnitTest()
        {
            var user = new User
            {
                Email = "inactive@example.com",
                LoginProvider = "Local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                IsEmailConfirmed = true,
                IsActive = false
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _userService.LoginAsync(user.Email, "123")
            );

            Assert.Equal("Account is deactivated.", ex.Message);
        }


        [Fact]
        public async Task Login_ShouldUpdateExistingRefreshToken_WhenTokenExists_UnitTest()
        {
            var user = new User
            {
                Email = "hasToken@example.com",
                LoginProvider = "Local",
                IsEmailConfirmed = true,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
                Role = new Role { RoleName = "User" }
            };

            var oldToken = new RefreshToken
            {
                UserEmail = user.Email,
                TokenHash = "oldhash",
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _refreshRepoMock.Setup(r => r.ExistingRefreshTokenAsync(user))
                            .ReturnsAsync(oldToken);

            var result = await _userService.LoginAsync(user.Email, "pass");

            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
            _refreshRepoMock.Verify(r => r.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
        }


        [Fact]
        public async Task Login_ShouldSetRefreshTokenExpiration_Correctly_UnitTest()
        {
            var user = new User
            {
                Email = "expiry@example.com",
                LoginProvider = "Local",
                IsEmailConfirmed = true,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("abc"),
                Role = new Role { RoleName = "User" }
            };

            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _refreshRepoMock.Setup(r => r.ExistingRefreshTokenAsync(user)).ReturnsAsync((RefreshToken)null);

            RefreshToken createdToken = null;

            _refreshRepoMock.Setup(r => r.CreateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                            .Callback<RefreshToken>(token => createdToken = token)
                            .Returns(Task.CompletedTask);

            var result = await _userService.LoginAsync(user.Email, "abc");

            Assert.NotNull(createdToken);
            var expectedExpiry = DateTime.UtcNow.AddDays(_jwtOptionsMock.Object.Value.RefreshTokenDays);
            Assert.True((createdToken.ExpiresAt - expectedExpiry).TotalMinutes < 1,
                "Refresh token expiration should be within 1 minute of expected value.");
        }


        [Fact]
        public async Task ChangePassword_Throws_WhenUserNotFound_UnitTest()
        {
            // Arrange
            string email = "notfound@example.com";
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync((User)null);

            var request = new ChangePasswordRequest { oldPass = "old", newPass = "new" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.ChangePasswordAsync(email, request));
        }

        [Fact]
        public async Task ChangePassword_Throws_WhenLoginProviderNotLocal_UnitTest()
        {
            // Arrange
            var user = new User { Email = "user@example.com", LoginProvider = "Google" };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var request = new ChangePasswordRequest { oldPass = "old", newPass = "new" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.ChangePasswordAsync(user.Email, request));
            Assert.Equal("Email is logged in with google, unable to change password", ex.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsFalse_WhenOldPasswordInvalid_UnitTest()
        {
            // Arrange
            var user = new User { Email = "user@example.com", LoginProvider = "Local", PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctOld") };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var request = new ChangePasswordRequest { oldPass = "wrongOld", newPass = "newPass" };

            // Act
            var result = await _userService.ChangePasswordAsync(user.Email, request);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ChangePassword_Throws_WhenOldPasswordEqualsNewPassword_UnitTest()
        {
            // Arrange
            var user = new User { Email = "user@example.com", LoginProvider = "Local", PasswordHash = BCrypt.Net.BCrypt.HashPassword("samePass") };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            var request = new ChangePasswordRequest { oldPass = "samePass", newPass = "samePass" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _userService.ChangePasswordAsync(user.Email, request));
            Assert.Equal("New password must be different from old password.", ex.Message);
        }

        [Fact]
        public async Task ChangePassword_UpdatesPassword_WhenValid_UnitTest()
        {
            // Arrange
            var user = new User { Email = "user@example.com", LoginProvider = "Local", PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldPass") };
            _userRepoMock.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

            User updatedUser = null;
            _userRepoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                         .Callback<User>(u => updatedUser = u)
                         .Returns(Task.CompletedTask);

            var request = new ChangePasswordRequest { oldPass = "oldPass", newPass = "newPass" };

            // Act
            var result = await _userService.ChangePasswordAsync(user.Email, request);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedUser);
            Assert.True(BCrypt.Net.BCrypt.Verify("newPass", updatedUser.PasswordHash));
            _userRepoMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        }



    }
}