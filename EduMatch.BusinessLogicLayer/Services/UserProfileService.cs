using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICloudMediaService _cloudMedia;
       
        private readonly IUserService _userService;
     
        public UserProfileService( 
            IUserProfileRepository repo,
            IMapper mapper,
            ICloudMediaService cloudMedia,
          
            IUserService userService)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudMedia = cloudMedia;
            
            _userService = userService;
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
		// Get email from request (passed from TutorProfileService)
		var userEmail = request.UserEmail;
		if (string.IsNullOrWhiteSpace(userEmail))
			throw new ArgumentException("User email is required.");

		var existingProfile = await _repo.GetByEmailAsync(userEmail);
		if (existingProfile == null)
			throw new InvalidOperationException($"User profile with email '{userEmail}' not found.");

		// Manual mapping: Request -> Entity
		if (request.Dob.HasValue)
			existingProfile.Dob = request.Dob;
		
		if (request.Gender.HasValue)
			existingProfile.Gender = (int)request.Gender;
		
		if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
			existingProfile.AvatarUrl = request.AvatarUrl;
		
		if (request.CityId.HasValue)
			existingProfile.CityId = request.CityId;
		
		if (request.SubDistrictId.HasValue)
			existingProfile.SubDistrictId = request.SubDistrictId;
		
		if (!string.IsNullOrWhiteSpace(request.AddressLine))
			existingProfile.AddressLine = request.AddressLine;
		
		if (request.Latitude.HasValue)
			existingProfile.Latitude = request.Latitude;
		
		if (request.Longitude.HasValue)
			existingProfile.Longitude = request.Longitude;

		await _repo.UpdateAsync(existingProfile);

		// Update User table (UserName, Phone) if provided
		if (!string.IsNullOrWhiteSpace(request.UserName) || !string.IsNullOrWhiteSpace(request.Phone))
		{
			await _userService.UpdateUserNameAndPhoneAsync(
				userEmail,
				request.Phone,
				request.UserName
			);
		}

		return _mapper.Map<UserProfileDto>(existingProfile);
	}

        public async Task<IEnumerable<ProvinceDto>> GetProvincesAsync()
        {
            var provinces = await _repo.GetProvincesAsync();
			return provinces.Select(u => new ProvinceDto
			{
				Id = u.Id,
				Name = u.Name
			});
			
        }

        public async Task<IEnumerable<SubDistrictDto>> GetSubDistrictsByProvinceIdAsync(int provinceId)
        {
            var subDistricts = await _repo.GetSubDistrictsByProvinceIdAsync(provinceId);
			return subDistricts.Select(u => new SubDistrictDto
			{
				Id= u.Id,
				Name = u.Name,
				ProvinceId = provinceId
			});
        }



    }
}
