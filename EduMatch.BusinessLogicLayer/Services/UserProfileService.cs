using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserProfileRepository _repo;
        private readonly IMapper _mapper;
       
        private readonly ICloudMediaService _cloudMedia;
        private readonly CurrentUserService _currentUserService;
     
        public UserProfileService( UserProfileRepository repo,
        IMapper mapper,
       
        ICloudMediaService cloudMedia,
       CurrentUserService currentUserService)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudMedia = cloudMedia;
            _currentUserService = currentUserService;
          
        }

        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _repo.GetByEmailAsync(email);
        }

		public async Task<UserProfileDto?> GetByEmailDatAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

			var profile = await _repo.GetByEmailAsync(email);
			if (profile == null) return null;

			return _mapper.Map<UserProfileDto>(profile);

		}



		public async Task<UserProfileDto?> UpdateAsync(UserProfileUpdateRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.UserEmail))
				throw new ArgumentException("User email is required.");

			var existingProfile = await _repo.GetByEmailAsync(request.UserEmail);
			if (existingProfile == null)
				throw new InvalidOperationException($"User profile with email '{request.UserEmail}' not found.");

			
			_mapper.Map(request, existingProfile);

			await _repo.UpdateAsync(existingProfile);

			return _mapper.Map<UserProfileDto>(existingProfile);
		}




        public async Task<bool> UpdateUserProfileAsync(string email, UpdateUserProfileRequest request)
        {
            var profile = await _repo.GetByEmailAsync(email);
            if (profile == null)
                throw new InvalidOperationException("User profile not found.");

            var user = profile.UserEmailNavigation;
            if (user == null)
                throw new InvalidOperationException("Associated user record not found.");

            if (request.AvatarFile != null && request.AvatarFile.Length > 0)
            {
                var userEmail = email;
                using var stream = request.AvatarFile.OpenReadStream();
                var uploadRequest = new UploadToCloudRequest(
                    Content: stream,
                    FileName: request.AvatarFile.FileName,
                    ContentType: request.AvatarFile.ContentType ?? "application/octet-stream",
                    LengthBytes: request.AvatarFile.Length,
                    OwnerEmail: userEmail,
                    MediaType: MediaType.Image
                );
                var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
                if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
                    throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");

                profile.AvatarUrl = uploadResult.SecureUrl;
                profile.AvatarUrlPublicId = uploadResult.PublicId;
            }


            // Update UserProfile fields
            profile.Dob = request.Dob ?? profile.Dob;
            profile.Gender = request.Gender ?? profile.Gender;
            profile.CityId = request.CityId ?? profile.CityId;
            profile.SubDistrictId = request.SubDistrictId ?? profile.SubDistrictId;
            profile.AddressLine = request.AddressLine ?? profile.AddressLine;


            // Update User fields
            if (!string.IsNullOrEmpty(request.UserName))
                user.UserName = request.UserName;
            if (!string.IsNullOrEmpty(request.Phone))
                user.Phone = request.Phone;

            // Save both with ONE transaction
            await _repo.UpdateUserProfileAndUserAsync(profile, user);
            return true;
        }

    }
}
