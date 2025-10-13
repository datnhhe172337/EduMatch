namespace EduMatch.PresentationLayer.Configurations
{
	public static class ServiceRegistration
	{
		public static IServiceCollection ConfigureApplication(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDependencies(configuration);
			services.AddCorsPolicy(configuration);
			services.AddJwtAuthentication(configuration);
			services.AddSwaggerWithJwt();
			services.AddApiBehavior();

			return services;
		}
	}
}
