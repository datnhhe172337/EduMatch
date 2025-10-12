using DotNetEnv;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Data;
using EduMatch.DataAccessLayer.Repositories;
using EduMatch.PresentationLayer.Configurations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// Load ENV 
Env.Load(); 
builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables();



// Add services to the container.
builder.Services.AddScoped<UserProfileRepository>();
builder.Services.AddScoped<TutorProfileRepository>();
builder.Services.ConfigureApplication(builder.Configuration);

builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<ITutorProfileService, TutorProfileService>();
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
