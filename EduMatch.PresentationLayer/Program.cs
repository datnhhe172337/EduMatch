using DotNetEnv;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.PresentationLayer.Configurations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load ENV 
Env.Load(); 
builder.Configuration
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables();

// Add services to the container.
builder.Services.ConfigureApplication(builder.Configuration);


// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<EduMatchContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("EduMatch")));

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// NHẬN header từ reverse proxy (rất quan trọng khi đứng sau Traefik/Coolify)
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
