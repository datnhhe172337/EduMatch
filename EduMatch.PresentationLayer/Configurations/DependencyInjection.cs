using DotNetEnv;
using EduMatch.BusinessLogicLayer.Settings;

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

		



			// HttpContextAccessor for CurrentUserService
			services.AddHttpContextAccessor();

			return services;
		}
	}
}
