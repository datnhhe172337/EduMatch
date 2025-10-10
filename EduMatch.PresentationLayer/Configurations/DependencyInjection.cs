using CloudinaryDotNet;
using DotNetEnv;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.BusinessLogicLayer.Utils;
using Microsoft.Extensions.Options;

namespace EduMatch.PresentationLayer.Configurations
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
		{
			//services.AddDbContext<PaymentDbContext>(options =>
			//	options.UseSqlServer(configuration.GetConnectionString("MyCnn")));

			//// Repositories
			//services.AddScoped<IInvoiceRepository, InvoiceRepository>();



			//// AutoMapper
			//services.AddAutoMapper(typeof(MappingProfile).Assembly);

			// inject HttpClient
			services.AddHttpClient();


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
