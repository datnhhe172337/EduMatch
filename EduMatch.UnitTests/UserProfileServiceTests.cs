using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.BusinessLogicLayer.Requests.User;

namespace EduMatch.Tests.Services
{
    public class UserProfileServiceTests
    {
        private readonly Mock<IUserProfileRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICloudMediaService> _mockCloudMedia;
        
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserProfileService _service;

        public UserProfileServiceTests()
        {
            _mockRepo = new Mock<IUserProfileRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCloudMedia = new Mock<ICloudMediaService>();
          
            _mockUserService = new Mock<IUserService>();

            _service = new UserProfileService(
                _mockRepo.Object,
                _mockMapper.Object,
                _mockCloudMedia.Object,
                
                _mockUserService.Object
            );
        }



        #region GetByEmailDatAsync Tests

        [Fact]
        public async Task GetByEmailDatAsync_ShouldReturnDto_WhenProfileIsFound()
        {
            // Arrange
            var email = "test@example.com";
            var fakeProfile = new UserProfile { UserEmail = email, CityId = 1 }; 
            var fakeDto = new UserProfileDto { UserEmail = email, ProvinceId = 1 }; 

            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(fakeProfile);
            _mockMapper.Setup(m => m.Map<UserProfileDto>(fakeProfile)).Returns(fakeDto);

            // Act
            var result = await _service.GetByEmailDatAsync(email);

            // Assert
            result.Should().Be(fakeDto);
        }
        //result.Should().Be(fakeDto);

        [Fact] 
        public async Task GetByEmailDatAsync_ShouldReturnNull_WhenProfileIsNotFound()
        {
            var email = "test@example.com";
            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((UserProfile)null);
            var result = await _service.GetByEmailDatAsync(email);
            result.Should().BeNull();
            _mockMapper.Verify(m => m.Map<UserProfileDto>(It.IsAny<UserProfile>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetByEmailDatAsync_ShouldThrowArgumentException_WhenEmailIsWhitespace(string invalidEmail)
        {
            Func<Task> act = async () => await _service.GetByEmailDatAsync(invalidEmail);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Email is required.");
        }

        [Fact]
        public async Task GetByEmailDatAsync_ShouldPropagateException_WhenRepoFails()
        {
            var email = "test@example.com";
            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ThrowsAsync(new InvalidOperationException("DB Error"));
            Func<Task> act = async () => await _service.GetByEmailDatAsync(email);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB Error");
        }


        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateProfileAndUser_WhenAllDataIsProvided()
        {
            // Arrange
            var email = "test@example.com";
            var request = new UserProfileUpdateRequest
            {
                UserEmail = email,
                Dob = new DateTime(1990, 1, 1),
                Gender = Gender.Male,
                AvatarUrl = "http://new.avatar.url",
                CityId = 1, 
                SubDistrictId = 101,
                AddressLine = "123 Main St",
                Latitude = 10.0m,
                Longitude = 20.0m,
                UserName = "New Name",
                Phone = "123456789"
            };
            var existingProfile = new UserProfile { UserEmail = email };
            var updatedDto = new UserProfileDto { UserEmail = email, FirstName = "New Name" }; 

            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingProfile);
            _mockRepo.Setup(r => r.UpdateAsync(existingProfile)).Returns(Task.CompletedTask);
            _mockUserService.Setup(u => u.UpdateUserNameAndPhoneAsync(email, "123456789", "New Name"))
                          .ReturnsAsync(new UserDto { Email = email }); 
            _mockMapper.Setup(m => m.Map<UserProfileDto>(existingProfile)).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAsync(request);

            // Assert
            result.Should().Be(updatedDto);
            existingProfile.Dob.Should().Be(request.Dob);
            existingProfile.Gender.Should().Be((int)request.Gender);
            existingProfile.AvatarUrl.Should().Be(request.AvatarUrl);
            existingProfile.CityId.Should().Be(request.CityId);
            existingProfile.SubDistrictId.Should().Be(request.SubDistrictId);
            existingProfile.AddressLine.Should().Be(request.AddressLine);
            existingProfile.Latitude.Should().Be(request.Latitude);
            existingProfile.Longitude.Should().Be(request.Longitude);

            _mockRepo.Verify(r => r.UpdateAsync(existingProfile), Times.Once);
            _mockUserService.Verify(u => u.UpdateUserNameAndPhoneAsync(email, "123456789", "New Name"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateProfileOnly_WhenUserDataIsMissing()
        {
            // Arrange
            var email = "test@example.com";
            var request = new UserProfileUpdateRequest
            {
                UserEmail = email,
                AddressLine = "456 New Ave",
                UserName = null,
                Phone = ""
            };
            var existingProfile = new UserProfile { UserEmail = email, AddressLine = "123 Old St" };
            
            var updatedDto = new UserProfileDto { UserEmail = email }; 
            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingProfile);
            _mockRepo.Setup(r => r.UpdateAsync(existingProfile)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserProfileDto>(existingProfile)).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAsync(request);

            // Assert
            result.Should().Be(updatedDto);
            existingProfile.AddressLine.Should().Be("456 New Ave");

            _mockRepo.Verify(r => r.UpdateAsync(existingProfile), Times.Once);
            _mockUserService.Verify(u => u.UpdateUserNameAndPhoneAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

       
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenEmailIsWhitespace(string invalidEmail)
        {
            var request = new UserProfileUpdateRequest { UserEmail = invalidEmail };
            Func<Task> act = async () => await _service.UpdateAsync(request);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("User email is required.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenProfileIsNotFound()
        {
            var email = "test@example.com";
            var request = new UserProfileUpdateRequest { UserEmail = email };
            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((UserProfile)null);
            Func<Task> act = async () => await _service.UpdateAsync(request);
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage($"User profile with email '{email}' not found.");
        }

        [Fact]
        public async Task UpdateAsync_ShouldPropagateException_WhenRepoUpdateFails()
        {
            var email = "test@example.com";
            var request = new UserProfileUpdateRequest { UserEmail = email, AddressLine = "123 Main St" };
            var existingProfile = new UserProfile { UserEmail = email };

            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingProfile);
            _mockRepo.Setup(r => r.UpdateAsync(existingProfile)).ThrowsAsync(new InvalidOperationException("DB Update Error"));

            Func<Task> act = async () => await _service.UpdateAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB Update Error");
        }

        [Fact]
        public async Task UpdateAsync_ShouldPropagateException_WhenUserServiceFails()
        {
            var email = "test@example.com";
            var request = new UserProfileUpdateRequest { UserEmail = email, UserName = "New Name" };
            var existingProfile = new UserProfile { UserEmail = email };

            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingProfile);
            _mockRepo.Setup(r => r.UpdateAsync(existingProfile)).Returns(Task.CompletedTask);
            _mockUserService.Setup(u => u.UpdateUserNameAndPhoneAsync(email, null, "New Name"))
                          .ThrowsAsync(new InvalidOperationException("User Service Error"));

            Func<Task> act = async () => await _service.UpdateAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("User Service Error");
        }

        #endregion
    }
}
