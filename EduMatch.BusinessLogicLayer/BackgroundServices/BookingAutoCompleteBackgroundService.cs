using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
	/// <summary>
	/// Background service tự động hoàn thành các Booking có status Confirmed khi tất cả schedule đều không còn Upcoming (chạy mỗi 1 giờ)
	/// </summary>
	public class BookingAutoCompleteBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

		public BookingAutoCompleteBackgroundService(IServiceProvider serviceProvider)
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
						var completedCount = await bookingService.AutoCompleteConfirmedBookingsAsync();
						if (completedCount > 0)
						{
							Console.WriteLine($"[BookingAutoComplete] Auto-completed {completedCount} confirmed bookings.");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[BookingAutoComplete] Error: {ex.Message}");
					}
				}

				await Task.Delay(Interval, stoppingToken);
			}
		}
	}
}

