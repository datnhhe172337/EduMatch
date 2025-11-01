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
        [EnumDataType(typeof(ScheduleStatus), ErrorMessage = "Status phải là giá trị hợp lệ của ScheduleStatus")]
        public ScheduleStatus? Status { get; set; }
        public string? AttendanceNote { get; set; }
        public bool? IsRefunded { get; set; }
    }
}
