using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System.Threading.Tasks;
using System;
using EduMatch.BusinessLogicLayer.Requests.User;

// Make sure your namespace matches your file's location
namespace EduMatch.BusinessLogicLayer.Services
{
    // This is now your main orchestration service
    public class ManageTutorProfileService : IManageTutorProfileService
    {
        // --- ADD ALL REPOSITORIES AND SERVICES YOU NEED ---
        private readonly ITutorProfileRepository _tutorProfileRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUserProfileRepository _userProfileRepo;
        private readonly IMapper _mapper;
        private readonly ICloudMediaService _cloudMedia;
        private readonly CurrentUserService _currentUserService;

        // --- INJECT ALL YOUR CHILD SERVICES ---
        private readonly ITutorEducationService _tutorEducationService;
        private readonly ITutorCertificateService _tutorCertificateService;
        private readonly ITutorSubjectService _tutorSubjectService;
        private readonly ITutorAvailabilityService _tutorAvailabilityService;
		private readonly IManageTutorProfileRepository _manageTutorProfileRepo;

        public ManageTutorProfileService(
            // --- ADD ALL THESE TO YOUR CONSTRUCTOR ---
            ITutorProfileRepository tutorProfileRepo, // You had IManageTutorProfileRepository, change it to the real one
            IUserRepository userRepo,
            IUserProfileRepository userProfileRepo,
            IMapper mapper,
            ICloudMediaService cloudMedia,
            CurrentUserService currentUserService,
            ITutorEducationService tutorEducationService,
            ITutorCertificateService tutorCertificateService,
            ITutorSubjectService tutorSubjectService,
            ITutorAvailabilityService tutorAvailabilityService,
            IManageTutorProfileRepository manageTutorProfileRepo)
        {
            _tutorProfileRepo = tutorProfileRepo;
            _userRepo = userRepo;
            _userProfileRepo = userProfileRepo;
            _mapper = mapper;
            _cloudMedia = cloudMedia;
            _currentUserService = currentUserService;
            _tutorEducationService = tutorEducationService;
            _tutorCertificateService = tutorCertificateService;
            _tutorSubjectService = tutorSubjectService;
            _tutorAvailabilityService = tutorAvailabilityService;
            _manageTutorProfileRepo = manageTutorProfileRepo;
        }

        // --- HELPER METHODS ---
        public async Task<TutorProfileDto?> GetByIdFullAsync(int id)
        {
            var profile = await _tutorProfileRepo.GetByIdFullAsync(id);
            return _mapper.Map<TutorProfileDto>(profile);
        }

        public async Task<TutorProfileDto?> GetByEmailAsync(string email)
        {
            var profile = await _manageTutorProfileRepo.GetByEmailAsync(email);
            return _mapper.Map<TutorProfileDto>(profile);
        }



		public Task<TutorProfileDto> UpdateTutorProfileAsync(int tutorId, TutorProfileUpdateRequest request)
		{
			throw new NotImplementedException();
		}

		// -----------------------------------------------------------------
		// --- THIS IS THE NEW ORCHESTRATOR METHOD ---
		// (It replaces your old, simple UpdateTutorProfileAsync)
		// -----------------------------------------------------------------
		//    public async Task<TutorProfileDto> UpdateTutorProfileAsync(int tutorId, UpdateTutorProfileRequest request)
		//    {
		//        // 1. Update Core Profile (if data was sent)
		//        if (request.Profile != null)
		//        {
		//            await UpdateCoreProfileDataAsync(tutorId, request.Profile);
		//        }

		//        // 2. Reconcile Child Lists (if data was sent)
		//        if (request.Educations != null)
		//        {

		//            AddressLine = dto.AddressLine,
		//            SubDistrictId = dto.SubDistrictId,
		//            CityId = dto.CityId,
		//            AvatarUrl = dto.AvatarUrl,
		//Gender = (int?)dto.Gender,
		//            Dob = dto.Dob
		//        };

		//            await _tutorEducationService.ReconcileAsync(tutorId, request.Educations);
		//        }


		//        if (request.Certificates != null)
		//        {
		//            await _tutorCertificateService.ReconcileAsync(tutorId, request.Certificates);
		//        }

		//        if (request.Subjects != null)
		//        {
		//            await _tutorSubjectService.ReconcileAsync(tutorId, request.Subjects);
		//        }

		//        if (request.Availabilities != null)
		//        {
		//            await _tutorAvailabilityService.ReconcileAsync(tutorId, request.Availabilities);
		//        }

		//        // 3. Return the fully updated data
		//        var updatedProfile = await _tutorProfileRepo.GetByIdFullAsync(tutorId);
		//        if (updatedProfile == null)
		//            throw new InvalidOperationException("Failed to retrieve updated profile.");

		//        return _mapper.Map<TutorProfileDto>(updatedProfile);

		//    }

		// --- PRIVATE HELPER for core data and files ---
		// --- PRIVATE HELPER for core data and files (CORRECTED) ---
		// --- PRIVATE HELPER for core data and files (CORRECTED AGAIN) ---
		//private async Task UpdateCoreProfileDataAsync(int tutorId, UpdateCoreTutorProfileRequest request)
		//{
		//    // --- STEP 1: Use the Get...ForUpdate methods to load WITH TRACKING ---
		//    // Ensure these methods exist in ALL THREE repos and DON'T use AsNoTracking()
		//    var tutorProfile = await _tutorProfileRepo.GetByIdForUpdateAsync(tutorId); // <<< CORRECTED CALL
		//    if (tutorProfile == null) throw new ArgumentException("Tutor profile not found.");

		//    var user = await _userRepo.GetUserByEmailAsync(tutorProfile.UserEmail); // <<< CORRECTED CALL (Ensure this exists in IUserRepository)
		//    var userProfile = await _userProfileRepo.GetByEmailAsync(tutorProfile.UserEmail); // <<< CORRECTED CALL (Ensure this exists in IUserProfileRepository)
		//    if (user == null || userProfile == null) throw new InvalidOperationException("User data is incomplete.");

		//    // Map non-null scalar properties onto the TRACKED entities
		//    _mapper.Map(request, tutorProfile);
		//    _mapper.Map(request, user);
		//    _mapper.Map(request, userProfile);

		//    // --- STEP 2: Handle File Uploads/Removals ---
		//    var oldAvatarId = userProfile.AvatarUrlPublicId;
		//    if (request.AvatarFile != null)
		//    {
		//        using var stream = request.AvatarFile.OpenReadStream();
		//        var uploadResult = await _cloudMedia.UploadAsync(new UploadToCloudRequest(
		//            Content: stream,
		//            FileName: request.AvatarFile.FileName,
		//            ContentType: request.AvatarFile.ContentType ?? "application/octet-stream",
		//            LengthBytes: request.AvatarFile.Length,
		//            OwnerEmail: _currentUserService.Email!,
		//            MediaType: MediaType.Image));
		//        if (!uploadResult.Ok) throw new InvalidOperationException("Avatar upload failed.");
		//        userProfile.AvatarUrl = uploadResult.SecureUrl;
		//        userProfile.AvatarUrlPublicId = uploadResult.PublicId;
		//    }
		//    else if (request.RemoveAvatar)
		//    {
		//        userProfile.AvatarUrl = null;
		//        userProfile.AvatarUrlPublicId = null;
		//    }

		//    var oldVideoId = tutorProfile.VideoIntroPublicId;
		//    if (request.VideoIntro != null)
		//    {
		//        using var stream = request.VideoIntro.OpenReadStream();
		//        var uploadResult = await _cloudMedia.UploadAsync(new UploadToCloudRequest(
		//            Content: stream,
		//            FileName: request.VideoIntro.FileName,
		//            ContentType: request.VideoIntro.ContentType ?? "application/octet-stream",
		//            LengthBytes: request.VideoIntro.Length,
		//            OwnerEmail: _currentUserService.Email!,
		//            MediaType: MediaType.Video));
		//        if (!uploadResult.Ok) throw new InvalidOperationException("Video upload failed.");
		//        tutorProfile.VideoIntroUrl = uploadResult.SecureUrl;
		//        tutorProfile.VideoIntroPublicId = uploadResult.PublicId;
		//    }
		//    else if (request.RemoveVideoIntro)
		//    {
		//        tutorProfile.VideoIntroUrl = null;
		//        tutorProfile.VideoIntroPublicId = null;
		//    }
		//    else if (request.VideoIntroUrl != null) // Handle URL-only
		//    {
		//        tutorProfile.VideoIntroUrl = string.IsNullOrWhiteSpace(request.VideoIntroUrl) ? null : request.VideoIntroUrl;
		//        tutorProfile.VideoIntroPublicId = null;
		//    }

		//    // --- STEP 3: Call SaveChangesAsync ONCE ---
		//    // This now saves the tracked changes correctly
		//    await _tutorProfileRepo.SaveChangesAsync();

		//    // --- STEP 4: Delete old files ---
		//    if ((request.AvatarFile != null || request.RemoveAvatar) && !string.IsNullOrWhiteSpace(oldAvatarId))
		//        _ = _cloudMedia.DeleteByPublicIdAsync(oldAvatarId, MediaType.Image);

		//    if ((request.VideoIntro != null || request.RemoveVideoIntro || request.VideoIntroUrl != null) && !string.IsNullOrWhiteSpace(oldVideoId))
		//        _ = _cloudMedia.DeleteByPublicIdAsync(oldVideoId, MediaType.Video);
		//}
	}
}



//public async Task<bool> UpdateTutorProfileAsync(string email, UpdateTutorProfileDto dto)
//{
//    var updatedProfile = new TutorProfile
//    {
//        Bio = dto.Bio,
//        TeachingExp = dto.TeachingExp,
//        VideoIntroUrl = dto.VideoIntroUrl
//    };

//    var updatedUserProfile = new UserProfile
//    {
//        AddressLine = dto.AddressLine,
//        SubDistrictId = dto.SubDistrictId,
//        CityId = dto.CityId,
//        AvatarUrl = dto.AvatarUrl,
//        Gender = dto.Gender,
//        Dob = dto.Dob
//    };

//    var result = await _tutorProfileRepo.UpdateTutorProfileAsync(email, updatedProfile, updatedUserProfile);
//    return result;
//}