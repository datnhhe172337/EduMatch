using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorProfileService : ITutorProfileService
    {
        private readonly TutorProfileRepository _repo;

        public TutorProfileService(TutorProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task<TutorProfile?> GetByEmailAsync(string email)
        {
            return await _repo.GetByEmailAsync(email);
        }

        public async Task<TutorProfile?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<bool> UpdateTutorProfileAsync(string email, UpdateTutorProfileDto dto)
        {
            var profile = await _repo.GetByEmailAsync(email);
            if (profile == null)
                return false;

            if (dto.Gender != null) profile.Gender = dto.Gender;
            if (dto.Dob.HasValue) profile.Dob = dto.Dob;
            if (dto.Title != null) profile.Title = dto.Title;
            if (dto.Bio != null) profile.Bio = dto.Bio;
            if (dto.TeachingExp != null) profile.TeachingExp = dto.TeachingExp;
            if (dto.VideoIntroUrl != null) profile.VideoIntroUrl = dto.VideoIntroUrl;
            if (dto.TeachingModes != null) profile.TeachingModes = dto.TeachingModes;

            profile.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(profile);
            return true;
        }
    }
}
