using System;
using System.ComponentModel.DataAnnotations;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.BusinessLogicLayer.Requests.MeetingSession
{
    public class MeetingSessionUpdateRequest
    {
        [Required(ErrorMessage = "Id là bắt buộc")]
        public int Id { get; set; }
        public int? ScheduleId { get; set; }
        [EmailAddress(ErrorMessage = "OrganizerEmail không đúng định dạng email")]
        public string? OrganizerEmail { get; set; }
        public string? MeetLink { get; set; }
        public string? MeetCode { get; set; }
        public string? EventId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        [EnumDataType(typeof(MeetingType), ErrorMessage = "MeetingType phải là giá trị hợp lệ của MeetingType")]
        public MeetingType? MeetingType { get; set; }
    }
}
