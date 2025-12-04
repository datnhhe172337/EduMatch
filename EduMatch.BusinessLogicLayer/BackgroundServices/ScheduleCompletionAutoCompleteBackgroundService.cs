using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
    public class ScheduleCompletionAutoCompleteBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

        public ScheduleCompletionAutoCompleteBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var completionService = scope.ServiceProvider.GetRequiredService<IScheduleCompletionService>();
                    try
                    {
                        var count = await completionService.AutoCompletePastDueAsync();
                        if (count > 0)
                        {
                            Console.WriteLine($"[ScheduleCompletionAutoComplete] Auto-completed {count} schedules.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ScheduleCompletionAutoComplete] Error: {ex.Message}");
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
