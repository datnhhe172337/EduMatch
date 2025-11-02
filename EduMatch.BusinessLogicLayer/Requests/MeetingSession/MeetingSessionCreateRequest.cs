using System;
using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.MeetingSession
{
    public class MeetingSessionCreateRequest
    {
        [Required(ErrorMessage = "ScheduleId là bắt buộc")]
        public int ScheduleId { get; set; }
    }
}
