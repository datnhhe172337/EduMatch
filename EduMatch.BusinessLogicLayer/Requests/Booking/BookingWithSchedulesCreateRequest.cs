using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduMatch.BusinessLogicLayer.Requests.Schedule;

namespace EduMatch.BusinessLogicLayer.Requests.Booking
{
    public class BookingWithSchedulesCreateRequest
    {
        [Required]
        public BookingCreateRequest Booking { get; set; } = new BookingCreateRequest();

        [Required]
        public List<ScheduleCreateRequest> Schedules { get; set; } = new List<ScheduleCreateRequest>();
    }
}


