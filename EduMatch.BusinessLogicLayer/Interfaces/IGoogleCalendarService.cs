using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface IGoogleCalendarService
	{
		/// <summary>
		/// Tạo sự kiện mới trên Google Calendar (kèm Google Meet link nếu có)
		/// </summary>
		Task<GoogleEventCreatedResponse?> CreateEventAsync(GoogleEventRequest request);

		/// <summary>
		/// Cập nhật thông tin sự kiện đã có trên Google Calendar
		/// </summary>
		Task<GoogleEventCreatedResponse?> UpdateEventAsync(string eventId, GoogleEventRequest request);

		/// <summary>
		/// Xóa sự kiện khỏi Google Calendar theo ID
		/// </summary>
		Task<bool> DeleteEventAsync(string eventId);
	}
}
