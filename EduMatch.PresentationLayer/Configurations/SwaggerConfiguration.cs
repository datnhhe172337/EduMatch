using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace EduMatch.PresentationLayer.Configurations
{
	public static class SwaggerConfiguration
	{
		public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
		{
			services.AddSwaggerGen(options =>
			{
				
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "EduMatch API",
					Version = "v1",
					Description = "EduMatch"
				});

				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
				options.IncludeXmlComments(xmlPath);





				// JWT Bearer Auth in Swagger
				var jwtSecurityScheme = new OpenApiSecurityScheme
				{
					BearerFormat = "JWT",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					Description = "Nhập token JWT vào ô bên dưới: Bearer {token}",

					Reference = new OpenApiReference
					{
						Id = JwtBearerDefaults.AuthenticationScheme,
						Type = ReferenceType.SecurityScheme
					}
				};

				options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

				options.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{ jwtSecurityScheme, new string[] { } }
				});
			});

			return services;
		}
	}
}
