using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

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
            var profile = await _repo.GetByEmailAsync(email);
            if (profile == null)
                return false;

            if (dto.AvatarUrl != null) profile.AvatarUrl = dto.AvatarUrl;
            if (dto.CityId.HasValue) profile.CityId = dto.CityId;
            if (dto.SubDistrictId.HasValue) profile.SubDistrictId = dto.SubDistrictId;
            if (dto.AddressLine != null) profile.AddressLine = dto.AddressLine;
            if (dto.Latitude.HasValue) profile.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) profile.Longitude = dto.Longitude;

            await _repo.UpdateAsync(profile);
            return true;
        }
    }
}
