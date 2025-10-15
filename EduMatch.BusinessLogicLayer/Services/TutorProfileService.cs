using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
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

		public TutorProfileService(
			 ITutorProfileRepository repository,
			 IMapper mapper,
			 ICloudMediaService cloudMedia,
			 CurrentUserService currentUserService
			 ) 
		{
			_repository = repository;
			_mapper = mapper;
			_cloudMedia = cloudMedia; 
			_currentUserService = currentUserService ;
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
				var existing = await _repository.GetByEmailFullAsync(request.UserEmail);
				if (existing is not null)
					throw new ArgumentException($"Tutor profile for email {request.UserEmail} already exists.");


				//  UploadToCloudRequest

				using var stream = request.VideoIntro.OpenReadStream();
				var uploadRequest = new UploadToCloudRequest(
					Content: stream,
					FileName: request.VideoIntro.FileName,
					ContentType: request.VideoIntro.ContentType ?? "application/octet-stream",
					LengthBytes: request.VideoIntro.Length,
					OwnerEmail: request.UserEmail,
					MediaType: MediaType.Video
				);


				//  CloudinaryMediaService.UploadAsync()

				var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);

				if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
					throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");


				// MAP  -> ENTITY

				var entity = new TutorProfile
				{
					UserEmail = request.UserEmail,
					Bio = request.Bio,
					TeachingExp = request.TeachingExp,
					VideoIntroUrl = uploadResult.SecureUrl,
					VideoIntroPublicId = uploadResult.PublicId,
					TeachingModes = request.TeachingModes,
					Status = TutorStatus.Pending,
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

		public async Task<TutorProfileDto> UpdateAsync(TutorProfileUpdateRequest request)
		{
			try
			{
				ValidateRequest(request);

				var existing = await _repository.GetByIdFullAsync(request.Id);
				if (existing is null)
					throw new ArgumentException($"Tutor profile with ID {request.Id} not found.");

				// Giữ lại publicId cũ để xoá sau khi update
				var oldPublicId = existing.VideoIntroPublicId;

				// Cập nhật fields
				existing.UserEmail = request.UserEmail;
				existing.Bio = request.Bio;
				existing.TeachingExp = request.TeachingExp;
				existing.VideoIntroUrl = request.VideoIntroUrl;
				existing.VideoIntroPublicId = request.VideoIntroPublicId;
				existing.TeachingModes = request.TeachingModes;
				existing.Status = request.Status;
				existing.UpdatedAt = DateTime.UtcNow;

				await _repository.UpdateAsync(existing);

				if (!string.IsNullOrWhiteSpace(oldPublicId))
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




		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
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
