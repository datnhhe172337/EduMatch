using DotNetEnv;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using EduMatch.PresentationLayer.Configurations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load ENV 
Env.TraversePath().Load();

builder.Configuration
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<UserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IManageTutorProfileRepository, ManageTutorProfileRepository>();
builder.Services.ConfigureApplication(builder.Configuration);
builder.Services.AddScoped<IFindTutorRepository, FindTutorRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IManageTutorProfileService, ManageTutorProfileService>();
builder.Services.AddScoped<IFindTutorService, FindTutorService>();

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddDbContext<EduMatchContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("EduMatch")));

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// NHẬN header từ reverse proxy 
app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Bật swagger theo ENV (an toàn cho prod)
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// KHÔNG ép redirect HTTPS ở Production nếu container chỉ nghe HTTP (proxy lo TLS)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection(); // chỉ dev
}

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

// Health endpoint để Coolify/Traefik check
app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();

app.Run();
