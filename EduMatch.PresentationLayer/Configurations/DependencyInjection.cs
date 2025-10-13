
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


namespace EduMatch.PresentationLayer.Configurations
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
		{
            //// Mail Settings
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));




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
            services.AddScoped<IEducationInstitutionLevelRepository, EducationInstitutionLevelRepository>();
            services.AddScoped<IEducationLevelRepository, EducationLevelRepository>();
            services.AddScoped<ILevelRepository, LevelRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();

            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<EmailService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<ITutorAvailabilityService, TutorAvailabilityService>();
            services.AddScoped<ITutorCertificateService, TutorCertificateService>();
            services.AddScoped<ITutorEducationService, TutorEducationService>();
            services.AddScoped<ITutorSubjectService, TutorSubjectService>();
            services.AddScoped<ICertificateTypeService, CertificateTypeService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<ITimeSlotService, TimeSlotService>();

            // HttpContextAccessor for CurrentUserService


            // Bind "CloudinarySettings" 
            services.Configure<CloudinaryRootOptions>(configuration.GetSection("CloudinarySettings"));

			services.AddSingleton(sp => {
				var opts = sp.GetRequiredService<IOptionsMonitor<CloudinaryRootOptions>>().CurrentValue;
				var acc = new Account(opts.Cloudinary.CloudName, opts.Cloudinary.ApiKey, opts.Cloudinary.ApiSecret);
				return new Cloudinary(acc) { Api = { Secure = true } };
			});
			services.AddSingleton<IMediaValidator, MediaValidator>();
			services.AddScoped<ICloudMediaService, CloudinaryMediaService>();





			// HttpContextAccessor for CurrentUserService
			services.AddHttpContextAccessor();

			return services;
		}
	}
}
