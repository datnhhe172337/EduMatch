using System;
using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.Schedule
{
    public class ScheduleUpdateRequest
    {
        [Required(ErrorMessage = "Id là bắt buộc")]
        public int Id { get; set; }
        public int? AvailabilitiId { get; set; }
        public int? BookingId { get; set; }
        public string? AttendanceNote { get; set; }
        public bool? IsRefunded { get; set; }

        [EnumDataType(typeof(ScheduleStatus), ErrorMessage = " ScheduleStatus  không hợp lệ ")]
        public ScheduleStatus? Status { get; set; }

		public bool? IsOnline { get; set; }
    }
}
