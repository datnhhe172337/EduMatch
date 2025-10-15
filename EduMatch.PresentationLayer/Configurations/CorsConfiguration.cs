namespace EduMatch.PresentationLayer.Configurations
{
	public static class CorsConfiguration
	{
		public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
		{
			var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();


			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy", builder =>
				{
					builder.WithOrigins(allowedOrigins)
						   .AllowAnyHeader()
						   .AllowAnyMethod()
						   .AllowCredentials();
				});
			});

			return services;
		}
	}
}
