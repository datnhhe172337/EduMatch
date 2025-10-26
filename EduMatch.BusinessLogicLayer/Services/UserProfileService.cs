using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.User;
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


		public Task<bool> UpdateUserProfileAsync(string email, UpdateUserProfileRequest request)
		{
			throw new NotImplementedException();
		}

	}
}
