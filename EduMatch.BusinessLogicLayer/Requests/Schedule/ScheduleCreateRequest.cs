using System;
using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.Schedule
{
    public class ScheduleCreateRequest
    {
        [Required(ErrorMessage = "AvailabilitiId là bắt buộc")]
        public int AvailabilitiId { get; set; }

        [Required(ErrorMessage = "BookingId là bắt buộc")]
        public int BookingId { get; set; }

        public string? AttendanceNote { get; set; }

        public bool IsOnline { get; set; } = true;
    }
}
