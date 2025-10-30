using System;
using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.MeetingSession
{
    public class MeetingSessionCreateRequest
    {
        [Required(ErrorMessage = "ScheduleId là bắt buộc")]
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "OrganizerEmail là bắt buộc")]
        [EmailAddress(ErrorMessage = "OrganizerEmail không đúng định dạng email")]
        public string OrganizerEmail { get; set; } = null!;

        [Required(ErrorMessage = "MeetLink là bắt buộc")]
        public string MeetLink { get; set; } = null!;

        public string? MeetCode { get; set; }
        public string? EventId { get; set; }

        [Required(ErrorMessage = "StartTime là bắt buộc")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "EndTime là bắt buộc")]
        public DateTime EndTime { get; set; }

        [EnumDataType(typeof(MeetingType), ErrorMessage = "MeetingType phải là giá trị hợp lệ của MeetingType")]
        public MeetingType MeetingType { get; set; } = MeetingType.Main;
    }
}
