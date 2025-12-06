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
	/// - Chuyển InProgress -> Completed nếu đã qua 24 giờ từ lúc bắt đầu học
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
						int inProgressToCompletedCount = 0;

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

							// Nếu đã qua 24 giờ từ lúc bắt đầu học vẫn InProgress thì chuyển sang Completed
							if (currentStatus == ScheduleStatus.InProgress)
							{
								// Tính thời gian đã trôi qua từ lúc bắt đầu học
								var timeElapsed = now - startDateTime;
								
								// Nếu đã qua 24 giờ (1 ngày)
								if (timeElapsed >= TimeSpan.FromHours(24))
								{
									try
									{
										await scheduleService.UpdateStatusAsync(schedule.Id, ScheduleStatus.Completed);
										inProgressToCompletedCount++;
										Console.WriteLine($"[ScheduleAutoStatusUpdate] Schedule {schedule.Id} chuyển từ InProgress sang Completed (đã qua 24 giờ từ lúc bắt đầu học)");
									}
									catch (Exception ex)
									{
										Console.WriteLine($"[ScheduleAutoStatusUpdate] Lỗi khi cập nhật Schedule {schedule.Id} sang Completed: {ex.Message}");
									}
								}
							}
						}

						if (upComingToInProgressCount > 0 || inProgressToCompletedCount > 0)
						{
							Console.WriteLine($"[ScheduleAutoStatusUpdate] Đã cập nhật: {upComingToInProgressCount} Upcoming->InProgress, {inProgressToCompletedCount} InProgress->Completed");
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

