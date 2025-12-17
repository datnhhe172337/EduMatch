using DotNetEnv;
using EduMatch.BusinessLogicLayer.BackgroundServices;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.PresentationLayer.Configurations;
using EduMatch.PresentationLayer.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Load ENV 
Env.TraversePath().Load();

builder.Configuration
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables();

// Add services to the container.

builder.Services.ConfigureApplication(builder.Configuration);

// Gemini
builder.Services.Configure<GeminiSettings>(
    builder.Configuration.GetSection("Gemini"));

builder.Services.Configure<QdrantSettings>(
    builder.Configuration.GetSection("Qdrant"));


// Background Service
builder.Services.AddHostedService<ClassRequestExpireBackgroundService>();

//builder.Services.AddHostedService<TutorSyncBackgroundService>();

builder.Services.AddHostedService<BookingAutoCancelBackgroundService>();
builder.Services.AddHostedService<BookingAutoCompleteBackgroundService>();
builder.Services.AddHostedService<ScheduleChangeRequestAutoCancelBackgroundService>();
builder.Services.AddHostedService<ScheduleAutoStatusUpdateBackgroundService>();
builder.Services.AddHostedService<ScheduleCompletionAutoCompleteBackgroundService>();
builder.Services.AddHostedService<TutorPayoutBackgroundService>();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});

builder.Services.AddDbContext<EduMatchContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("EduMatch")));


builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");
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
