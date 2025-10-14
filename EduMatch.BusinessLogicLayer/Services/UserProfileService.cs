using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Repositories;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserProfileRepository _repo;

        public UserProfileService(UserProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _repo.GetByEmailAsync(email);
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
