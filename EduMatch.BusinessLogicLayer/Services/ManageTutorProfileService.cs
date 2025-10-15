using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ManageTutorProfileService : IManageTutorProfileService
    {
        private readonly IManageTutorProfileRepository _tutorProfileRepo;

        public ManageTutorProfileService(IManageTutorProfileRepository tutorProfileRepo)
        {
            _tutorProfileRepo = tutorProfileRepo;          
        }

        public async Task<TutorProfile?> GetByEmailAsync(string email)
        {
            return await _tutorProfileRepo.GetByEmailAsync(email);
        }

        public async Task<TutorProfile?> GetByIdAsync(int id)
        {
            return await _tutorProfileRepo.GetByIdAsync(id);
        }

        public async Task<bool> UpdateTutorProfileAsync(string email, UpdateTutorProfileDto dto)
        {
            var updatedProfile = new TutorProfile
            {
                Bio = dto.Bio,
                TeachingExp = dto.TeachingExp,
                VideoIntroUrl = dto.VideoIntroUrl
            };

            var updatedUserProfile = new UserProfile
            {
                AddressLine = dto.AddressLine,
                SubDistrictId = dto.SubDistrictId,
                CityId = dto.CityId,
                AvatarUrl = dto.AvatarUrl,
                Gender = dto.Gender,
                Dob = dto.Dob
            };

            var result = await _tutorProfileRepo.UpdateTutorProfileAsync(email, updatedProfile, updatedUserProfile);
            return result;
        }
    }
}
