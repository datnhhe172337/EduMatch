using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
	/// <summary>
	/// Background service tự động hủy các Booking chưa được xác nhận (chạy mỗi 1 giờ)
	/// </summary>
	public class BookingAutoCancelBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

		public BookingAutoCancelBackgroundService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using (var scope = _serviceProvider.CreateScope())
				{
					var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

					try
					{
						var cancelledCount = await bookingService.AutoCancelUnconfirmedBookingsAsync();
						if (cancelledCount > 0)
						{
							Console.WriteLine($"[BookingAutoCancel] Cancelled {cancelledCount} pending bookings.");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[BookingAutoCancel] Error: {ex.Message}");
					}
				}

				await Task.Delay(Interval, stoppingToken);
			}
		}
	}
}

