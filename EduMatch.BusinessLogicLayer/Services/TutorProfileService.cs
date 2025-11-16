using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Requests.Common;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System;

namespace EduMatch.BusinessLogicLayer.Services
{
	public sealed class TutorProfileService : ITutorProfileService
	{
		private readonly ITutorProfileRepository _tutorProfileRepository;
		private readonly ICloudMediaService _cloudMedia;
		private readonly IMapper _mapper;
		private readonly CurrentUserService _currentUserService;
		private readonly IUserService _userService;
		private readonly IUserProfileService _userProfileService;

		public TutorProfileService(
			ITutorProfileRepository repository,
			 IMapper mapper,
			 ICloudMediaService cloudMedia,
			 CurrentUserService currentUserService,
			 IUserService userService,
			 IUserProfileService userProfileService
			 ) 
		{
			_tutorProfileRepository = repository;
			_mapper = mapper;
			_cloudMedia = cloudMedia; 
			_currentUserService = currentUserService ;
			_userService = userService;
			_userProfileService = userProfileService;
		}



		/// <summary>
		/// Lấy TutorProfile theo ID với đầy đủ thông tin
		/// </summary>
		public async Task<TutorProfileDto?> GetByIdFullAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			var entity = await _tutorProfileRepository.GetByIdFullAsync(id);
			return entity is null ? null : _mapper.Map<TutorProfileDto>(entity);
		}

		/// <summary>
		/// Lấy TutorProfile theo Email với đầy đủ thông tin
		/// </summary>
		public async Task<TutorProfileDto?> GetByEmailFullAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Email is required.");

			var entity = await _tutorProfileRepository.GetByEmailFullAsync(email);
			return entity is null ? null : _mapper.Map<TutorProfileDto>(entity);
		}

		/// <summary>
		/// Lấy tất cả TutorProfile với đầy đủ thông tin
		/// </summary>
		public async Task<IReadOnlyList<TutorProfileDto>> GetAllFullAsync()
		{
			var entities = await _tutorProfileRepository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorProfileDto>>(entities);
		}

		/// <summary>
		/// Tạo TutorProfile mới
		/// </summary>
		public async Task<TutorProfileDto> CreateAsync(TutorProfileCreateRequest request)
		{
			try
			{
				// CHECK IF TUTOR PROFILE EXISTS
				if (string.IsNullOrWhiteSpace(_currentUserService.Email))
					throw new ArgumentException("Current user email not found.");
				var userEmail = _currentUserService.Email!;
				var existing = await _tutorProfileRepository.GetByEmailFullAsync(userEmail);
				if (existing is not null)
					throw new ArgumentException($"Tutor profile for email {userEmail} already exists.");

				// Validate URLs
				if (string.IsNullOrWhiteSpace(request.VideoIntroUrl))
					throw new ArgumentException("VideoIntroUrl is required.");
				
				if (string.IsNullOrWhiteSpace(request.AvatarUrl))
					throw new ArgumentException("AvatarUrl is required.");

				// Process video URL - YouTube or regular video link
				string finalVideoUrl;
				if (IsYouTubeUrl(request.VideoIntroUrl!))
				{
					// If it's YouTube, normalize to embed format
					var normalizedVideoUrl = NormalizeYouTubeEmbedUrlOrNull(request.VideoIntroUrl!);
					if (normalizedVideoUrl is null)
						throw new ArgumentException("VideoIntroUrl must be a valid YouTube link.");
					finalVideoUrl = normalizedVideoUrl;
				}
				else
				{
					// If it's not YouTube, use the original URL
					finalVideoUrl = request.VideoIntroUrl!;
				}

				// Update user profile with avatar URL
				var userProfileUpdate = new UserProfileUpdateRequest
				{
					UserEmail = userEmail, 
					UserName = request.UserName,
					Phone = request.Phone,
					Dob = request.DateOfBirth,
					CityId = request.ProvinceId,
					SubDistrictId = request.SubDistrictId,
					AvatarUrl = request.AvatarUrl,
					Latitude = request.Latitude,
					Longitude = request.Longitude
				};
				await _userProfileService.UpdateAsync(userProfileUpdate);

				// MAP  -> ENTITY
				var entity = new TutorProfile
				{
					UserEmail = userEmail,
					Bio = request.Bio,
					TeachingExp = request.TeachingExp,
					VideoIntroUrl = finalVideoUrl,
					VideoIntroPublicId = null, // No public ID for external URLs
					TeachingModes = (int)request.TeachingModes,
					Status = (int)TutorStatus.Pending,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _tutorProfileRepository.AddAsync(entity);
				return _mapper.Map<TutorProfileDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor profile: {ex.Message}", ex);
			}
		}

		


	
			/// <summary>
		/// Cập nhật TutorProfile
		/// </summary>
	public async Task<TutorProfileDto> UpdateAsync(TutorProfileUpdateRequest request)
		{
			try
			{
			// Get email from current user service
			var userEmail = _currentUserService.Email;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ArgumentException("Current user email not found.");

			var existing = await _tutorProfileRepository.GetByIdFullAsync(request.Id);
			if (existing is null)
				throw new ArgumentException($"Tutor profile with ID {request.Id} not found.");

			// Update user profile with new data if provided
			var userProfileUpdate = new UserProfileUpdateRequest
			{
				UserEmail = userEmail,  
				UserName = request.UserName ?? existing.UserEmailNavigation?.UserName,
				Phone = request.Phone ?? existing.UserEmailNavigation?.Phone,
				Dob = request.DateOfBirth ?? existing.UserEmailNavigation?.UserProfile?.Dob,
				CityId = request.ProvinceId ?? existing.UserEmailNavigation?.UserProfile?.CityId,
				SubDistrictId = request.SubDistrictId ?? existing.UserEmailNavigation?.UserProfile?.SubDistrictId,
				AvatarUrl = request.AvatarUrl ?? existing.UserEmailNavigation?.UserProfile?.AvatarUrl,
				Latitude = request.Latitude ?? existing.UserEmailNavigation?.UserProfile?.Latitude,
				Longitude = request.Longitude ?? existing.UserEmailNavigation?.UserProfile?.Longitude
			};
			await _userProfileService.UpdateAsync(userProfileUpdate);

				// Update tutor profile fields - only if provided
				if (!string.IsNullOrWhiteSpace(request.Bio))
					existing.Bio = request.Bio;

				if (!string.IsNullOrWhiteSpace(request.TeachingExp))
					existing.TeachingExp = request.TeachingExp;

				// Update video URL if provided
				if (!string.IsNullOrWhiteSpace(request.VideoIntroUrl))
				{
					string finalVideoUrl;
					if (IsYouTubeUrl(request.VideoIntroUrl!))
					{
						// If it's YouTube, normalize to embed format
						var normalized = NormalizeYouTubeEmbedUrlOrNull(request.VideoIntroUrl!);
						if (normalized is null)
							throw new ArgumentException("VideoIntroUrl must be a valid YouTube link.");
						finalVideoUrl = normalized;
					}
					else
					{
						// If it's not YouTube, use the original URL
						finalVideoUrl = request.VideoIntroUrl!;
					}
					existing.VideoIntroUrl = finalVideoUrl;
					existing.VideoIntroPublicId = null; // No public ID for external URLs
				}

			// Update teaching modes if provided
			existing.TeachingModes = (int)request.TeachingModes;

			// Update status if provided - only allow progression from Pending to other statuses
			if (request.Status.HasValue)
			{
				var currentStatus = (TutorStatus)existing.Status;
				var newStatus = request.Status.Value;
				
				// Only allow status progression: Pending -> Approved/Rejected, not backwards
				if (currentStatus == TutorStatus.Pending && (newStatus == TutorStatus.Approved || newStatus == TutorStatus.Rejected))
				{
					existing.Status = (int)newStatus;
				}
				else if (currentStatus != TutorStatus.Pending)
				{
					// If not Pending, don't allow status changes
					throw new ArgumentException($"Cannot change status from {currentStatus} to {newStatus}. Status can only be changed from Pending to Approved/Rejected.");
				}
				else
				{
					throw new ArgumentException($"Invalid status change from {currentStatus} to {newStatus}. Only Pending -> Approved/Rejected is allowed.");
				}
			}

			existing.UpdatedAt = DateTime.UtcNow;

			await _tutorProfileRepository.UpdateAsync(existing);
			return _mapper.Map<TutorProfileDto>(existing);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor profile: {ex.Message}", ex);
			}
		}




		/// <summary>
		/// Xóa TutorProfile theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			await _tutorProfileRepository.RemoveByIdAsync(id);
		}

		/// <summary>
		/// Xác thực TutorProfile
		/// </summary>
		public async Task<TutorProfileDto> VerifyAsync(int id, string verifiedBy)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("ID must be greater than 0");

				if (string.IsNullOrWhiteSpace(verifiedBy))
					throw new ArgumentException("VerifiedBy is required");

				// Check if entity exists
			var existingEntity = await _tutorProfileRepository.GetByIdFullAsync(id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Tutor profile with ID {id} not found");
				}

				// Check if current status is Pending
				if (existingEntity.Status != (int)TutorStatus.Pending)
				{
					throw new InvalidOperationException($"Tutor profile with ID {id} is not in Pending status for verification");
				}

				// Update verification status
				existingEntity.Status = (int)TutorStatus.Approved;
				existingEntity.VerifiedBy = verifiedBy;
				existingEntity.VerifiedAt = DateTime.UtcNow;
				existingEntity.UpdatedAt = DateTime.UtcNow;

			await _tutorProfileRepository.UpdateAsync(existingEntity);
				return _mapper.Map<TutorProfileDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to verify tutor profile: {ex.Message}", ex);
			}
		}

		

		private static bool IsYouTubeUrl(string url)
		{
			try
			{
				var uri = new Uri(url);
				return uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
					   uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private static string? NormalizeYouTubeEmbedUrlOrNull(string rawUrl)
		{
			try
			{
				var uri = new Uri(rawUrl);
				string? videoId = null;
				if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
				{
					videoId = uri.AbsolutePath.Trim('/');
				}
				else if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
				{
					var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
					videoId = query["v"];
					if (string.IsNullOrWhiteSpace(videoId) && uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
						videoId = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
				}
				if (string.IsNullOrWhiteSpace(videoId)) return null;
				return $"https://www.youtube.com/embed/{videoId}";
			}
			catch
			{
				return null;
			}
		}

        public async Task<IReadOnlyList<TutorProfileDto>> GetTutorsUpdatedAfterAsync(DateTime lastSync)
        {
            var entities = await _tutorProfileRepository.GetTutorsUpdatedAfterAsync(lastSync);
            return _mapper.Map<IReadOnlyList<TutorProfileDto>>(entities);
        }

    }
}
