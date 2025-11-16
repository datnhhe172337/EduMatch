using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
    public class ClassRequestExpireBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClassRequestExpireBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var expireService = scope.ServiceProvider.GetRequiredService<IClassRequestService>();

                    try
                    {
                        await expireService.ExpireClassRequestsAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ExpireService] Error: {ex.Message}");
                    }
                }

                // Chạy mỗi 24h 
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
