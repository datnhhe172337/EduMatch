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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
	public sealed class TutorProfileService : ITutorProfileService
	{
		private readonly ITutorProfileRepository _repository;
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
			_repository = repository;
			_mapper = mapper;
			_cloudMedia = cloudMedia; 
			_currentUserService = currentUserService ;
			_userService = userService;
			_userProfileService = userProfileService;
		}



		public async Task<TutorProfileDto?> GetByIdFullAsync(int id)
		{
			var entity = await _repository.GetByIdFullAsync(id);
			return entity is null ? null : _mapper.Map<TutorProfileDto>(entity);
		}


		public async Task<TutorProfileDto?> GetByEmailFullAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Email is required.");

			var entity = await _repository.GetByEmailFullAsync(email);
			return entity is null ? null : _mapper.Map<TutorProfileDto>(entity);
		}

		public async Task<IReadOnlyList<TutorProfileDto>> GetAllFullAsync()
		{
			var entities = await _repository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorProfileDto>>(entities);
		}



		
		/*
		public async Task<TutorProfileDto> CreateAsync(TutorProfileCreateRequest request)
		{
			try
			{

				// VALIDATE REQUEST 

				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException(
						$"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}"
					);
				}

				//  CHECK IF TUTOR PROFILE EXISTS
				if (string.IsNullOrWhiteSpace(_currentUserService.Email))
					throw new ArgumentException("Current user email not found.");
				var userEmail = _currentUserService.Email!;
				var existing = await _repository.GetByEmailFullAsync(userEmail);
				if (existing is not null)
					throw new ArgumentException($"Tutor profile for email {userEmail} already exists.");


				//  UploadToCloudRequest

				string? videoUrl = null;
				string? videoPublicId = null;
				var hasFileVideo = request.VideoIntro != null && request.VideoIntro.Length > 0 && !string.IsNullOrWhiteSpace(request.VideoIntro.FileName);
				if (hasFileVideo)
				{
					using var stream = request.VideoIntro!.OpenReadStream();
					var uploadRequest = new UploadToCloudRequest(
						Content: stream,
						FileName: request.VideoIntro!.FileName,
						ContentType: request.VideoIntro!.ContentType ?? "application/octet-stream",
						LengthBytes: request.VideoIntro!.Length,
						OwnerEmail: userEmail,
						MediaType: MediaType.Video
					);
					var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
					if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
						throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
					videoUrl = uploadResult.SecureUrl;
					videoPublicId = uploadResult.PublicId;
				}
				else if (!string.IsNullOrWhiteSpace(request.VideoIntroUrl))
				{
					var normalized = NormalizeYouTubeEmbedUrlOrNull(request.VideoIntroUrl!);
					if (normalized is null)
						throw new ArgumentException("VideoIntroUrl must be a valid YouTube link.");
					videoUrl = normalized;
				}
				else
				{
					throw new ArgumentException("Either VideoIntro file or VideoIntroUrl is required.");
				}
				string? AvataUrl = null;
				string? AvataUrlcId = null;
				var hasFileAvata = request.AvatarFile != null && request.AvatarFile.Length > 0 && !string.IsNullOrWhiteSpace(request.AvatarFile.FileName);
				if (hasFileAvata)
				{
					using var stream = request.AvatarFile!.OpenReadStream();
					var uploadAvataRequest = new UploadToCloudRequest(
						Content: stream,
						FileName: request.AvatarFile!.FileName,
						ContentType: request.AvatarFile!.ContentType ?? "application/octet-stream",
						LengthBytes: request.AvatarFile!.Length,
						OwnerEmail: userEmail,
						MediaType: MediaType.Image
					);
					var uploadAvataResult = await _cloudMedia.UploadAsync(uploadAvataRequest);
					if (!uploadAvataResult.Ok || string.IsNullOrEmpty(uploadAvataResult.SecureUrl))
						throw new InvalidOperationException($"Failed to upload file: {uploadAvataResult.ErrorMessage}");
					AvataUrl = uploadAvataResult.SecureUrl;
					AvataUrlcId = uploadAvataResult.PublicId;
				}
				else
				{
					throw new ArgumentException("Either AvataUrl file  is required.");
				}
				
				var userProfileUpdate = new UserProfileUpdateRequest
				{
					UserEmail = _currentUserService.Email,
					Dob = request.DateOfBirth,
					CityId = request.ProvinceId,
					SubDistrictId = request.SubDistrictId,
					AvatarUrl = AvataUrl,
					AvatarUrlPublicId = AvataUrlcId
				};
				await _userProfileService.UpdateAsync(userProfileUpdate);



				await _userService.UpdateUserNameAndPhoneAsync(_currentUserService.Email,request.Phone,request.UserName);

				// MAP  -> ENTITY

				var entity = new TutorProfile
				{
					UserEmail = userEmail,
					Bio = request.Bio,
					TeachingExp = request.TeachingExp,
					VideoIntroUrl = videoUrl,
					VideoIntroPublicId = videoPublicId,
					TeachingModes = (int?)request.TeachingModes,
					Status = (int?)TutorStatus.Pending,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _repository.AddAsync(entity);



				return _mapper.Map<TutorProfileDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor profile: {ex.Message}", ex);
			}
		}
		*/

		
		public async Task<TutorProfileDto> CreateAsync(TutorProfileCreateRequest request)
		{
			try
			{
				// VALIDATE REQUEST 
				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException(
						$"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}"
					);
				}

				//  CHECK IF TUTOR PROFILE EXISTS
				if (string.IsNullOrWhiteSpace(_currentUserService.Email))
					throw new ArgumentException("Current user email not found.");
				var userEmail = _currentUserService.Email!;
				var existing = await _repository.GetByEmailFullAsync(userEmail);
				if (existing is not null)
					throw new ArgumentException($"Tutor profile for email {userEmail} already exists.");

				// Validate URLs
				if (string.IsNullOrWhiteSpace(request.VideoIntroUrl))
					throw new ArgumentException("VideoIntroUrl is required.");
				
				if (string.IsNullOrWhiteSpace(request.AvatarUrl))
					throw new ArgumentException("AvatarUrl is required.");

				// Normalize YouTube URL
				var normalizedVideoUrl = NormalizeYouTubeEmbedUrlOrNull(request.VideoIntroUrl!);
				if (normalizedVideoUrl is null)
					throw new ArgumentException("VideoIntroUrl must be a valid YouTube link.");

				// Update user profile with avatar URL
				var userProfileUpdate = new UserProfileUpdateRequest
				{
					UserEmail = _currentUserService.Email,
					Dob = request.DateOfBirth,
					CityId = request.ProvinceId,
					SubDistrictId = request.SubDistrictId,
					AvatarUrl = request.AvatarUrl,
					AvatarUrlPublicId = null // No public ID for external URLs
				};
				await _userProfileService.UpdateAsync(userProfileUpdate);

				await _userService.UpdateUserNameAndPhoneAsync(_currentUserService.Email, request.Phone, request.UserName);

				// MAP  -> ENTITY
				var entity = new TutorProfile
				{
					UserEmail = userEmail,
					Bio = request.Bio,
					TeachingExp = request.TeachingExp,
					VideoIntroUrl = normalizedVideoUrl,
					VideoIntroPublicId = null, // No public ID for external URLs
					TeachingModes = (int)request.TeachingModes,
					Status = (int)TutorStatus.Pending,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _repository.AddAsync(entity);
				return _mapper.Map<TutorProfileDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor profile: {ex.Message}", ex);
			}
		}

		
		/*
		public async Task<TutorProfileDto> UpdateAsync(TutorProfileUpdateRequest request)
		{
			try
			{
				ValidateRequest(request);

				var existing = await _repository.GetByIdFullAsync(request.Id);
				if (existing is null)
					throw new ArgumentException($"Tutor profile with ID {request.Id} not found.");

				var oldPublicId = existing.VideoIntroPublicId;

				// Update only provided fields
				if (!string.IsNullOrWhiteSpace(request.Bio))
					existing.Bio = request.Bio;
				if (!string.IsNullOrWhiteSpace(request.TeachingExp))
					existing.TeachingExp = request.TeachingExp;

				var hasNewFile = request.VideoIntro != null && request.VideoIntro.Length > 0 && !string.IsNullOrWhiteSpace(request.VideoIntro.FileName);
				if (hasNewFile)
				{
					using var stream = request.VideoIntro!.OpenReadStream();
					var uploadRequest = new UploadToCloudRequest(
						Content: stream,
						FileName: request.VideoIntro!.FileName,
						ContentType: request.VideoIntro!.ContentType ?? "application/octet-stream",
						LengthBytes: request.VideoIntro!.Length,
						OwnerEmail: _currentUserService.Email!,
						MediaType: MediaType.Video
					);
					var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
					if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
						throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
					existing.VideoIntroUrl = uploadResult.SecureUrl;
					existing.VideoIntroPublicId = uploadResult.PublicId;
				}
				else if (!string.IsNullOrWhiteSpace(request.VideoIntroUrl))
				{
					var normalized = NormalizeYouTubeEmbedUrlOrNull(request.VideoIntroUrl!);
					if (normalized is null)
						throw new ArgumentException("VideoIntroUrl must be a valid YouTube link.");
					existing.VideoIntroUrl = normalized;
					// keep old public id when only URL changes
				}

				if (request.TeachingModes.HasValue)
					existing.TeachingModes = (int?)request.TeachingModes.Value;

				if (request.Status.HasValue)
					existing.Status = (int?)request.Status.Value;

				existing.UpdatedAt = DateTime.UtcNow;

				await _repository.UpdateAsync(existing);

				if (hasNewFile && !string.IsNullOrWhiteSpace(oldPublicId))
				{
					_ = _cloudMedia.DeleteByPublicIdAsync(oldPublicId, MediaType.Video)
						.ContinueWith(t =>
						{
							if (t.IsCompletedSuccessfully)
							{
								Console.WriteLine($" Xóa video {oldPublicId} thành công.");
							}
							else if (t.IsFaulted)
							{
								Console.WriteLine($" Xóa video {oldPublicId} thất bại: {t.Exception?.GetBaseException().Message}");
							}
						});
				}


				return _mapper.Map<TutorProfileDto>(existing);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor profile: {ex.Message}", ex);
			}
		}
		*/

	
		public async Task<TutorProfileDto> UpdateAsync(TutorProfileUpdateRequest request)
		{
			try
			{
				ValidateRequest(request);

				var existing = await _repository.GetByIdFullAsync(request.Id);
				if (existing is null)
					throw new ArgumentException($"Tutor profile with ID {request.Id} not found.");

				// Update only provided fields
				if (!string.IsNullOrWhiteSpace(request.Bio))
					existing.Bio = request.Bio;
				if (!string.IsNullOrWhiteSpace(request.TeachingExp))
					existing.TeachingExp = request.TeachingExp;

				// Update video URL if provided
				if (!string.IsNullOrWhiteSpace(request.VideoIntroUrl))
				{
					var normalized = NormalizeYouTubeEmbedUrlOrNull(request.VideoIntroUrl!);
					if (normalized is null)
						throw new ArgumentException("VideoIntroUrl must be a valid YouTube link.");
					existing.VideoIntroUrl = normalized;
					existing.VideoIntroPublicId = null; // No public ID for external URLs
				}

				if (request.TeachingModes.HasValue)
					existing.TeachingModes = (int)request.TeachingModes.Value;

				if (request.Status.HasValue)
					existing.Status = (int)request.Status.Value;

				existing.UpdatedAt = DateTime.UtcNow;

				await _repository.UpdateAsync(existing);
				return _mapper.Map<TutorProfileDto>(existing);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor profile: {ex.Message}", ex);
			}
		}




		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
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

		private static void ValidateRequest(object request)
		{
			var ctx = new ValidationContext(request);
			var results = new List<ValidationResult>();
			if (!Validator.TryValidateObject(request, ctx, results, true))
			{
				var msg = string.Join(", ", results.Select(r => r.ErrorMessage));
				throw new ArgumentException($"Validation failed: {msg}");
			}
		}
	}
}
