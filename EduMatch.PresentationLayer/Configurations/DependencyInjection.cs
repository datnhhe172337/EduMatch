using CloudinaryDotNet;
using DotNetEnv;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.BusinessLogicLayer.Utils;
using EduMatch.BusinessLogicLayer.Mappings;
using Microsoft.Extensions.Options;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using Microsoft.AspNetCore.SignalR;


namespace EduMatch.PresentationLayer.Configurations
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
		{
            //// Mail Settings
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

            services.Configure<VnpaySettings>(configuration.GetSection("VnpaySettings"));

            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

			// inject HttpClient
			services.AddHttpClient();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepositoy, RefreshTokenRepository>();
            services.AddScoped<ITutorProfileRepository, TutorProfileRepository>();
            services.AddScoped<ITutorAvailabilityRepository, TutorAvailabilityRepository>();
            services.AddScoped<ITutorCertificateRepository, TutorCertificateRepository>();
            services.AddScoped<ITutorEducationRepository, TutorEducationRepository>();
            services.AddScoped<ITutorSubjectRepository, TutorSubjectRepository>();
            services.AddScoped<ICertificateTypeRepository, CertificateTypeRepository>();
            services.AddScoped<ICertificateTypeSubjectRepository, CertificateTypeSubjectRepository>();
            services.AddScoped<IEducationInstitutionRepository, EducationInstitutionRepository>();
            services.AddScoped<ILevelRepository, LevelRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IFavoriteTutorRepository, FavoriteTutorRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<UserProfileRepository, UserProfileRepository>();
            services.AddScoped<IManageTutorProfileRepository, ManageTutorProfileRepository>();
            services.AddScoped<IFindTutorRepository, FindTutorRepository>();
            
            services.AddScoped<IClassRequestRepository, ClassRequestRepository>();
            services.AddScoped<ITutorApplicationRepository, TutorApplicationRepository>();
            services.AddScoped<ISystemFeeRepository, SystemFeeRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingNoteRepository, BookingNoteRepository>();
            services.AddScoped<IBookingNoteMediaRepository, BookingNoteMediaRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IScheduleChangeRequestRepository, ScheduleChangeRequestRepository>();
            services.AddScoped<IMeetingSessionRepository, MeetingSessionRepository>();

            services.AddScoped<IBookingRefundRequestRepository, BookingRefundRequestRepository>();
            services.AddScoped<IRefundRequestEvidenceRepository, RefundRequestEvidenceRepository>();
            services.AddScoped<ITutorVerificationRequestRepository, TutorVerificationRequestRepository>();

            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReportDefenseRepository, ReportDefenseRepository>();
            services.AddScoped<IReportEvidenceRepository, ReportEvidenceRepository>();
            services.AddScoped<IScheduleCompletionRepository, ScheduleCompletionRepository>();
            services.AddScoped<ITutorPayoutRepository, TutorPayoutRepository>();


            services.AddScoped<ITutorFeedbackRepository, TutorFeedbackRepository>();
			
			services.AddScoped<UserProfileRepository, UserProfileRepository>();
			services.AddScoped<IManageTutorProfileRepository, ManageTutorProfileRepository>();
			
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IBankRepository, BankRepository>();
            services.AddScoped<IUserBankAccountRepository, UserBankAccountRepository>();
            services.AddScoped<IDepositRepository, DepositRepository>();
            services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
            services.AddScoped<IWithdrawalRepository, WithdrawalRepository>();


            services.AddScoped<IChatbotRepository, ChatbotRepository>();

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IRefundPolicyRepository, RefundPolicyRepository>();
            services.AddScoped<ITutorRatingSummaryRepository, TutorRatingSummaryRepository>();
            services.AddScoped<ILearnerTrialLessonRepository, LearnerTrialLessonRepository>();

            // Services
            services.AddScoped<IRefundPolicyService, RefundPolicyService>();
			services.AddScoped<IUserService, UserService>();
            services.AddScoped<CurrentUserService>();
			services.AddTransient<EmailService>();
			services.AddScoped<IGoogleTokenRepository, GoogleTokenRepository>();
			services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<ITutorAvailabilityService, TutorAvailabilityService>();
            services.AddScoped<ITutorCertificateService, TutorCertificateService>();
            services.AddScoped<ITutorEducationService, TutorEducationService>();
            services.AddScoped<ITutorSubjectService, TutorSubjectService>();
            services.AddScoped<ITutorProfileService, TutorProfileService>();
			services.AddScoped<ICertificateTypeService, CertificateTypeService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<ITimeSlotService, TimeSlotService>();
            services.AddScoped<IEducationInstitutionService, EducationInstitutionService>();
            services.AddScoped<IFavoriteTutorService, FavoriteTutorService>();
            services.AddScoped<ISystemFeeService, SystemFeeService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IBookingNoteService, BookingNoteService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IScheduleChangeRequestService, ScheduleChangeRequestService>();
            services.AddScoped<IMeetingSessionService, MeetingSessionService>();

            services.AddScoped<IBookingRefundRequestService, BookingRefundRequestService>();
            services.AddScoped<IRefundRequestEvidenceService, RefundRequestEvidenceService>();
            services.AddScoped<ITutorVerificationRequestService, TutorVerificationRequestService>();

            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportContentValidator, ReportContentValidator>();


            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IManageTutorProfileService, ManageTutorProfileService>();
            services.AddScoped<IFindTutorService, FindTutorService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IClassRequestService, ClassRequestService>();
            services.AddScoped<ITutorApplicationService, TutorApplicationService>();
			services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();



            services.AddScoped<ISystemFeeRepository, SystemFeeRepository>();
            services.AddScoped<ChatService>();

            services.AddScoped<ITutorFeedbackService, TutorFeedbackService>();
			
			
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationPusher, NotificationPusher>();


            services.AddSingleton<IUserIdProvider, EmailUserIdProvider>();


            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IUserBankAccountService, UserBankAccountService>();
            services.AddScoped<IDepositService, DepositService>();
            services.AddScoped<IVnpayService, VnpayService>();
            services.AddScoped<IWithdrawalService, WithdrawalService>();
            services.AddScoped<IAdminWalletService, AdminWalletService>();
            services.AddScoped<ITutorRatingSummaryService, TutorRatingSummaryService>();
            services.AddScoped<IAdminStatsService, AdminStatsService>();
            services.AddScoped<ITutorDashboardService, TutorDashboardService>();
            services.AddScoped<ILearnerTrialLessonService, LearnerTrialLessonService>();
            services.AddScoped<ITutorPayoutService, TutorPayoutService>();
            services.AddScoped<IScheduleCompletionService, ScheduleCompletionService>();
                            // Services for Chatbot AI
            services.AddScoped<IGeminiChatService, GeminiChatService>();
            services.AddSingleton<IEmbeddingService, EmbeddingService>();
            services.AddSingleton<IQdrantService, QdrantService>();
            services.AddScoped<IChatbotService, ChatbotService>();
            services.AddSingleton<IPromptService, PromptService>();


            // Bind "CloudinarySettings" 
            services.Configure<CloudinaryRootOptions>(configuration.GetSection("CloudinarySettings"));

			services.AddSingleton(sp => {
				var opts = sp.GetRequiredService<IOptionsMonitor<CloudinaryRootOptions>>().CurrentValue;
				var acc = new Account(opts.Cloudinary.CloudName, opts.Cloudinary.ApiKey, opts.Cloudinary.ApiSecret);
				return new Cloudinary(acc) { Api = { Secure = true } };
			});
			services.AddSingleton<IMediaValidator, MediaValidator>();
			services.AddScoped<ICloudMediaService, CloudinaryMediaService>();

			// Cấu hình cho Google Calendar API
			services.Configure<GoogleCalendarSettings>(configuration.GetSection("GoogleCalendarSettings"));


			//  HttpClient 
			//services.AddHttpClient();


			// HttpContextAccessor for CurrentUserService
			services.AddHttpContextAccessor();

			return services;
		}
	}
}
