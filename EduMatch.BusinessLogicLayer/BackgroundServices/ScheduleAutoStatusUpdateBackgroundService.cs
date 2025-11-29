using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.BackgroundServices
{
	/// <summary>
	/// Background service tự động cập nhật trạng thái Schedule:
	/// - Chuyển Upcoming -> InProgress khi đến giờ bắt đầu học
	/// - Chuyển InProgress -> Cancelled sau 12h đêm nếu vẫn còn InProgress
	/// </summary>
	public class ScheduleAutoStatusUpdateBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5); // Chạy mỗi 5 phút

		public ScheduleAutoStatusUpdateBackgroundService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Lấy thời gian hiện tại theo giờ Việt Nam (UTC+7)
		/// </summary>
		private DateTime GetVietnamTime()
		{
			return DateTime.UtcNow.AddHours(7);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using (var scope = _serviceProvider.CreateScope())
				{
					var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
					var scheduleRepository = scope.ServiceProvider.GetRequiredService<EduMatch.DataAccessLayer.Interfaces.IScheduleRepository>();

					try
					{
						var now = GetVietnamTime(); // Sử dụng giờ Việt Nam
						var schedules = (await scheduleRepository.GetAllUpcomingAndInProgressAsync()).ToList();

						int upComingToInProgressCount = 0;
						int inProgressToCancelledCount = 0;

						foreach (var schedule in schedules)
						{
							// Kiểm tra nếu có đầy đủ thông tin Availability và Slot
							if (schedule.Availabiliti == null || schedule.Availabiliti.Slot == null)
								continue;

							var startDate = schedule.Availabiliti.StartDate.Date;
							var startTime = schedule.Availabiliti.Slot.StartTime;
							var startDateTime = startDate.Add(startTime.ToTimeSpan());

							var currentStatus = (ScheduleStatus)schedule.Status;

							// Chuyển Upcoming -> InProgress khi đến giờ bắt đầu
							if (currentStatus == ScheduleStatus.Upcoming && now >= startDateTime)
							{
								try
								{
									await scheduleService.UpdateStatusAsync(schedule.Id, ScheduleStatus.InProgress);
									upComingToInProgressCount++;
									Console.WriteLine($"[ScheduleAutoStatusUpdate] Schedule {schedule.Id} chuyển từ Upcoming sang InProgress");
								}
								catch (Exception ex)
								{
									Console.WriteLine($"[ScheduleAutoStatusUpdate] Lỗi khi cập nhật Schedule {schedule.Id} sang InProgress: {ex.Message}");
								}
							}

							// Sau 12h đêm của ngày học, chuyển InProgress -> Cancelled nếu vẫn còn InProgress
							// Kiểm tra nếu đã qua 12h đêm của ngày học (tức là đã qua ngày đó)
							if (currentStatus == ScheduleStatus.InProgress)
							{
								var todayMidnight = now.Date; // 00:00:00 của ngày hiện tại
								var scheduleDayMidnight = startDate; // 00:00:00 của ngày học
								
								// Nếu đã qua 12h đêm của ngày học (tức là đã sang ngày hôm sau)
								if (todayMidnight > scheduleDayMidnight)
								{
									try
									{
										await scheduleService.UpdateStatusAsync(schedule.Id, ScheduleStatus.Cancelled);
										inProgressToCancelledCount++;
										Console.WriteLine($"[ScheduleAutoStatusUpdate] Schedule {schedule.Id} chuyển từ InProgress sang Cancelled (đã qua 12h đêm của ngày học)");
									}
									catch (Exception ex)
									{
										Console.WriteLine($"[ScheduleAutoStatusUpdate] Lỗi khi cập nhật Schedule {schedule.Id} sang Cancelled: {ex.Message}");
									}
								}
							}
						}

						if (upComingToInProgressCount > 0 || inProgressToCancelledCount > 0)
						{
							Console.WriteLine($"[ScheduleAutoStatusUpdate] Đã cập nhật: {upComingToInProgressCount} Upcoming->InProgress, {inProgressToCancelledCount} InProgress->Cancelled");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[ScheduleAutoStatusUpdate] Error: {ex.Message}");
					}
				}

				await Task.Delay(Interval, stoppingToken);
			}
		}
	}
}

