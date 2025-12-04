using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Requests.Common;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Globalization;
using System.Text;


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

		private readonly IQdrantService _qdrantService;

		private readonly ITutorVerificationRequestService _tutorVerificationRequestService;
		private readonly EduMatchContext _context;

		public TutorProfileService(
			ITutorProfileRepository repository,
			 IMapper mapper,
			 ICloudMediaService cloudMedia,
			 CurrentUserService currentUserService,
			 IUserService userService,
			 IUserProfileService userProfileService, 
			 IQdrantService qdrantService,
			 ITutorVerificationRequestService tutorVerificationRequestService,
			 EduMatchContext context
		) 
		{
			_tutorProfileRepository = repository;
			_mapper = mapper;
			_cloudMedia = cloudMedia; 
			_currentUserService = currentUserService ;
			_userService = userService;
			_userProfileService = userProfileService;
			_qdrantService = qdrantService;
			_tutorVerificationRequestService = tutorVerificationRequestService;
			_context = context;
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
			// Kiểm tra xem đã có transaction từ bên ngoài chưa
			var existingTransaction = _context.Database.CurrentTransaction;
			var shouldManageTransaction = existingTransaction == null;
			
			Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? dbTransaction = null;
			if (shouldManageTransaction)
			{
				dbTransaction = await _context.Database.BeginTransactionAsync();
			}
			
			try
			{
				// CHECK IF TUTOR PROFILE EXISTS
				if (string.IsNullOrWhiteSpace(request.UserEmail))
					throw new Exception("Email người dùng không được để trống");
				var userEmail = request.UserEmail;
				var existing = await _tutorProfileRepository.GetByEmailFullAsync(userEmail);
				
				// Validate URLs
				if (string.IsNullOrWhiteSpace(request.VideoIntroUrl))
					throw new Exception("VideoIntroUrl là bắt buộc");
				
				if (string.IsNullOrWhiteSpace(request.AvatarUrl))
					throw new Exception("AvatarUrl là bắt buộc");

				// Process video URL - YouTube or regular video link
				string finalVideoUrl;
				if (IsYouTubeUrl(request.VideoIntroUrl!))
				{
					// If it's YouTube, normalize to embed format
					var normalizedVideoUrl = NormalizeYouTubeEmbedUrlOrNull(request.VideoIntroUrl!);
					if (normalizedVideoUrl is null)
						throw new Exception("VideoIntroUrl phải là link YouTube hợp lệ");
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

				// Nếu email đã tồn tại, kiểm tra status và TutorVerificationRequest
				if (existing is not null)
				{
					var existingStatus = (TutorStatus)existing.Status;
					
					// Nếu status là Pending hoặc Rejected, check TutorVerificationRequest
					if (existingStatus == TutorStatus.Pending || existingStatus == TutorStatus.Rejected)
					{
						// Check xem có đơn TutorVerificationRequest nào đang Pending không
						var verificationRequests = await _tutorVerificationRequestService.GetAllByEmailOrTutorIdAsync(
							email: userEmail,
							tutorId: existing.Id,
							status: TutorVerificationRequestStatus.Pending);
						
						if (verificationRequests != null && verificationRequests.Any())
						{
							throw new Exception("Đang chờ duyệt trở thành gia sư");
						}
						
						// Nếu không có đơn Pending, UPDATE lại profile hiện tại
						// Update các trường
						existing.Bio = request.Bio;
						existing.TeachingExp = request.TeachingExp;
						existing.VideoIntroUrl = finalVideoUrl;
						existing.VideoIntroPublicId = null;
						existing.TeachingModes = (int)request.TeachingModes;
						existing.Status = (int)TutorStatus.Pending;
						existing.UpdatedAt = DateTime.UtcNow;

						await _tutorProfileRepository.UpdateAsync(existing);

						// Tạo TutorVerificationRequest mới với TutorId và Email
						var verificationRequest = new TutorVerificationRequestCreateRequest
						{
							UserEmail = userEmail,
							TutorId = existing.Id,
							Description = "Yêu cầu xác minh gia sư tự động tạo khi đăng ký hồ sơ"
						};

					await _tutorVerificationRequestService.CreateAsync(verificationRequest);

					// Commit transaction chỉ khi service tự quản lý transaction
					if (shouldManageTransaction && dbTransaction != null)
					{
						await dbTransaction.CommitAsync();
					}

					return _mapper.Map<TutorProfileDto>(existing);
					}
					
					// Nếu status là Approved, không cho tạo mới
					if (existingStatus == TutorStatus.Approved)
					{
						throw new Exception($"Hồ sơ gia sư với email {userEmail} đã tồn tại và đã được duyệt");
					}
					
					// Các trường hợp khác (Suspended, Deactivated)
					throw new Exception($"Hồ sơ gia sư với email {userEmail} đã tồn tại");
				}

				// Nếu chưa tồn tại, tạo mới entity
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

				// Tạo TutorVerificationRequest với TutorId và Email
				var newVerificationRequest = new TutorVerificationRequestCreateRequest
				{
					UserEmail = userEmail,
					TutorId = entity.Id,
					Description = "Yêu cầu xác minh gia sư tự động tạo khi đăng ký hồ sơ"
				};

				await _tutorVerificationRequestService.CreateAsync(newVerificationRequest);

				// Commit transaction chỉ khi service tự quản lý transaction
				if (shouldManageTransaction && dbTransaction != null)
				{
					await dbTransaction.CommitAsync();
				}

				return _mapper.Map<TutorProfileDto>(entity);
			}
			catch (Exception ex)
			{
				// Rollback transaction chỉ khi service tự quản lý transaction
				if (shouldManageTransaction && dbTransaction != null)
				{
					await dbTransaction.RollbackAsync();
				}
				throw new Exception($"Lỗi khi tạo hồ sơ gia sư: {ex.Message}", ex);
			}
			finally
			{
				// Dispose transaction chỉ khi service tự tạo
				if (shouldManageTransaction && dbTransaction != null)
				{
					await dbTransaction.DisposeAsync();
				}
			}
		}

		


	
			/// <summary>
		/// Cập nhật TutorProfile
		/// </summary>
	public async Task<TutorProfileDto> UpdateAsync(TutorProfileUpdateRequest request)
		{
			using var dbTransaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// Get email from current user service
				var userEmail = _currentUserService.Email;
				if (string.IsNullOrWhiteSpace(userEmail))
					throw new Exception("Không tìm thấy email người dùng hiện tại");

				var existing = await _tutorProfileRepository.GetByIdFullAsync(request.Id);
				if (existing is null)
					throw new Exception($"Không tìm thấy hồ sơ gia sư với ID {request.Id}");

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
							throw new Exception("VideoIntroUrl phải là link YouTube hợp lệ");
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

				existing.UpdatedAt = DateTime.UtcNow;

				await _tutorProfileRepository.UpdateAsync(existing);

				// Commit transaction
				await dbTransaction.CommitAsync();

				return _mapper.Map<TutorProfileDto>(existing);
			}
			catch (Exception ex)
			{
				// Rollback transaction nếu có lỗi
				await dbTransaction.RollbackAsync();
				throw new Exception($"Lỗi khi cập nhật hồ sơ gia sư: {ex.Message}", ex);
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
		/// Cập nhật Status của TutorProfile (chỉ cho phép từ Approved sang Suspended hoặc Deactivated)
		/// </summary>
		public async Task<TutorProfileDto> UpdateStatusAsync(int id, TutorStatus status)
		{
			using var dbTransaction = await _context.Database.BeginTransactionAsync();
			try
			{
				if (id <= 0)
					throw new Exception("Id phải lớn hơn 0");

				// Check if entity exists
				var existingEntity = await _tutorProfileRepository.GetByIdFullAsync(id);
				if (existingEntity == null)
					throw new Exception($"Không tìm thấy hồ sơ gia sư với ID {id}");

				var currentStatus = (TutorStatus)existingEntity.Status;

				// Chỉ cho phép update từ Approved sang Suspended hoặc Deactivated
				if (currentStatus != TutorStatus.Approved)
				{
					throw new Exception($"Không thể cập nhật trạng thái. Trạng thái hiện tại là {currentStatus}. Chỉ có thể cập nhật từ trạng thái Đã duyệt sang Tạm khóa hoặc Ngừng hoạt động. Trạng thái Đã duyệt và Bị từ chối chỉ được thay đổi khi được xác nhận thành gia sư hoặc từ chối.");
				}

				// Chỉ cho phép chuyển sang Suspended hoặc Deactivated
				if (status != TutorStatus.Suspended && status != TutorStatus.Deactivated)
				{
					throw new Exception($"Không thể cập nhật trạng thái sang {status}. Chỉ có thể cập nhật từ Đã duyệt sang Tạm khóa hoặc Ngừng hoạt động.");
				}

				// Update status
				existingEntity.Status = (int)status;
				existingEntity.UpdatedAt = DateTime.UtcNow;

				await _tutorProfileRepository.UpdateAsync(existingEntity);

				// Commit transaction
				await dbTransaction.CommitAsync();

				return _mapper.Map<TutorProfileDto>(existingEntity);
			}
			catch (Exception ex)
			{
				// Rollback transaction nếu có lỗi
				await dbTransaction.RollbackAsync();
				throw new Exception($"Lỗi khi cập nhật trạng thái hồ sơ gia sư: {ex.Message}", ex);
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

		public async Task<int> SyncAllTutorsAsync()
		{
			var tutors = await _tutorProfileRepository.GetAllFullAsync();
            if (!tutors.Any())
                return 0;

			var tutorsDto = _mapper.Map<IReadOnlyList<TutorProfileDto>>(tutors);

            await _qdrantService.UpsertTutorsAsync(tutorsDto);

            // 4. Cập nhật LastSync
            var now = DateTime.UtcNow;

            foreach (var tutor in tutors)
            {
                tutor.LastSync = now;
            }

            await _tutorProfileRepository.SaveChangesAsync();

            return tutors.Count;
        }

        public Task<TutorProfileDto> VerifyAsync(int id, string verifiedBy)
        {
            throw new NotImplementedException();
        }

        //     public async Task<List<(TutorProfileDto Tutor, float Score)>> SearchByKeywordAsync(string keyword)
        //     {
        //if (string.IsNullOrWhiteSpace(keyword))
        //	return new List<(TutorProfileDto Tutor, float Score)>();

        //         string q = keyword.Trim().ToLower();

        //         var tutors = await _tutorProfileRepository.GetAllFullAsync();
        //         var tutorsDto = _mapper.Map<IReadOnlyList<TutorProfileDto>>(tutors);

        //         var results = new List<(TutorProfileDto, float)>();

        //         var result = tutorsDto
        //             .Where(t =>
        //                 // Search in subjects
        //                 t.TutorSubjects != null && t.TutorSubjects.Any(s =>
        //                     !string.IsNullOrEmpty(s.Subject?.SubjectName) &&
        //                     s.Subject.SubjectName.ToLower().Contains(q)
        //                 )
        //		//  Search in levels
        //		|| t.TutorSubjects != null && t.TutorSubjects.Any(s =>
        //                     !string.IsNullOrEmpty(s.Level?.Name) &&
        //                     s.Level.Name.ToLower().Contains(q)
        //                 )
        //                 // Or search in Province
        //                 || (!string.IsNullOrEmpty(t.Province?.Name) && t.Province.Name.ToLower().Contains(q))
        //                 // Or search in SubDistrict
        //                 || (!string.IsNullOrEmpty(t.SubDistrict?.Name) && t.SubDistrict.Name.ToLower().Contains(q))
        //             )
        //             .Take(top)
        //             .ToList();

        //         return result;
        //     }

        public async Task<List<(TutorProfileDto Tutor, float Score)>> SearchByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<(TutorProfileDto Tutor, float Score)>();

            // --- Normalize & tokenize ---
            string q = NormalizeText(keyword);
            var tokens = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Load all tutors (full)
            var tutors = await _tutorProfileRepository.GetAllFullAsync();
            var tutorsDto = _mapper.Map<IReadOnlyList<TutorProfileDto>>(tutors);

            var results = new List<(TutorProfileDto, float)>();

            foreach (var t in tutorsDto)
            {
                float score = 0f;

                // Normalize data fields
                string province = NormalizeText(t.Province?.Name);
                string subDistrict = NormalizeText(t.SubDistrict?.Name);
                string bio = NormalizeText(t.Bio);
                string teaching = NormalizeText(t.TeachingExp);
                var subjects = t.TutorSubjects?.Select(s => NormalizeText(s.Subject?.SubjectName)).ToList() ?? new();
                var levels = t.TutorSubjects?.Select(s => NormalizeText(s.Level?.Name)).ToList() ?? new();

                foreach (string token in tokens)
                {
                    if (subjects.Any(s => s.Contains(token))) score += 5;
                    if (levels.Any(l => l.Contains(token))) score += 2;
                    if (!string.IsNullOrEmpty(province) && province.Contains(token)) score += 1;
                    if (!string.IsNullOrEmpty(subDistrict) && subDistrict.Contains(token)) score += 1;
                }

                if (score >= 5)
                    results.Add((t, score));
            }

            return results;
        }


        public static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Trim().ToLower();

            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in normalized)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }


    }
}
