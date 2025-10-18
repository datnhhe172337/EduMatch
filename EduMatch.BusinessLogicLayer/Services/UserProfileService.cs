using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Repositories;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserProfileRepository _repo;
        private readonly IMapper _mapper;

		public UserProfileService(UserProfileRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
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




		public async Task<bool> UpdateUserProfileAsync(string email, UpdateUserProfileDto dto)
        {
            // Load profile including linked User entity
            var profile = await _repo.GetByEmailAsync(email);
            if (profile == null)
                return false;

            var user = profile.UserEmailNavigation; // Related User entity

            // --- Update UserProfile fields ---
            if (dto.Dob.HasValue)
                profile.Dob = dto.Dob.Value;

            if (dto.Gender.HasValue)
                profile.Gender = dto.Gender.Value;

            if (!string.IsNullOrEmpty(dto.AvatarUrl))
                profile.AvatarUrl = dto.AvatarUrl;

            if (!string.IsNullOrEmpty(dto.AvatarUrlPublicId))
                profile.AvatarUrlPublicId = dto.AvatarUrlPublicId;

            if (dto.CityId.HasValue)
                profile.CityId = dto.CityId;

            if (dto.SubDistrictId.HasValue)
                profile.SubDistrictId = dto.SubDistrictId;

            if (!string.IsNullOrEmpty(dto.AddressLine))
                profile.AddressLine = dto.AddressLine;

            if (dto.Latitude.HasValue)
                profile.Latitude = dto.Latitude;

            if (dto.Longitude.HasValue)
                profile.Longitude = dto.Longitude;

            // --- Update User table fields ---
            if (user != null)
            {
                if (!string.IsNullOrEmpty(dto.UserName))
                    user.UserName = dto.UserName;

                if (!string.IsNullOrEmpty(dto.Phone))
                    user.Phone = dto.Phone;
            }

            await _repo.UpdateAsync(profile);
            return true;
        }
    }
}
