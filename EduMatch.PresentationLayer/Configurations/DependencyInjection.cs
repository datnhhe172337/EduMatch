using DotNetEnv;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;

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
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            //// AutoMapper
            //services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // HttpContextAccessor for CurrentUserService


            // Bind "CloudinarySettings" 
            services.Configure<CloudinaryRootOptions>(configuration.GetSection("CloudinarySettings"));

		



			// HttpContextAccessor for CurrentUserService
			services.AddHttpContextAccessor();

			return services;
		}
	}
}
