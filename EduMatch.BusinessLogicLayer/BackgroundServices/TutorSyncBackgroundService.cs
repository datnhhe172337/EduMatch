using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
    public class TutorSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private DateTime _lastSyncTime = DateTime.UtcNow.AddMinutes(-30);
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(15); // chạy mỗi 15 phút

        public TutorSyncBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ITutorProfileService>();
                    var qdrant = scope.ServiceProvider.GetRequiredService<IQdrantService>();

                    var updatedTutors = await repo.GetTutorsUpdatedAfterAsync(_lastSyncTime);

                    if (updatedTutors.Any())
                    {
                        await qdrant.UpsertTutorsAsync(updatedTutors);
                        _lastSyncTime = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TutorSync] Error: {ex.Message}");
                    // không throw để service vẫn chạy tiếp
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
