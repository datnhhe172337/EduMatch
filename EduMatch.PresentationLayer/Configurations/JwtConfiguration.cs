using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace EduMatch.PresentationLayer.Configurations
{
	public static class JwtConfiguration
	{
		public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
            // Bind section JwtSettings vào IOptions
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            // Lấy giá trị thực tế (merge JSON + env)
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);

            services.AddSingleton(jwtSettings);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Cấu hình Authentication JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                //lưu lại token JWT (từ header Authorization) vào HttpContext -> await HttpContext.GetTokenAsync("access_token");
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    /*Nếu không set, token hết hạn vẫn dùng được thêm 5 phút mặc định*/
                    ClockSkew = TimeSpan.Zero // loại bỏ thời gian trễ hạn token mặc định 5 phút
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/chatHub"))) // Must match your app.MapHub
                        {
                            // ...read the token from the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
                ;
            });

            return services;
		}
	}
}
