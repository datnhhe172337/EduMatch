using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
	/// <summary>
	/// Background service tự động hủy các ScheduleChangeRequest Pending quá 3 ngày hoặc sắp đến giờ học (chạy mỗi 1 giờ)
	/// </summary>
	public class ScheduleChangeRequestAutoCancelBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly TimeSpan Interval = TimeSpan.FromHours(1); // Chạy mỗi 1 giờ

		public ScheduleChangeRequestAutoCancelBackgroundService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using (var scope = _serviceProvider.CreateScope())
				{
					var scheduleChangeRequestService = scope.ServiceProvider.GetRequiredService<IScheduleChangeRequestService>();

					try
					{
						var cancelledCount = await scheduleChangeRequestService.AutoCancelExpiredPendingRequestsAsync();
						if (cancelledCount > 0)
						{
							Console.WriteLine($"[ScheduleChangeRequestAutoCancel] Cancelled {cancelledCount} expired pending requests.");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[ScheduleChangeRequestAutoCancel] Error: {ex.Message}");
					}
				}

				await Task.Delay(Interval, stoppingToken);
			}
		}
	}
}

