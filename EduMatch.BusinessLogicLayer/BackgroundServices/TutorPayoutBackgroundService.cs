using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
    public class TutorPayoutBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        public TutorPayoutBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var payoutService = scope.ServiceProvider.GetRequiredService<ITutorPayoutService>();
                    try
                    {
                        var count = await payoutService.ProcessDuePayoutsAsync();
                        if (count > 0)
                        {
                            Console.WriteLine($"[TutorPayoutBackgroundService] Released {count} tutor payouts.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TutorPayoutBackgroundService] Error: {ex.Message}");
                    }
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // stopping
                }
            }
        }
    }
}
