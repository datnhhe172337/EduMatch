using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ClassRequestDetailDto
    {
        public int Id { get; set; }
        public string LearnerEmail { get; set; } 
        public string Title { get; set; }
        public string SubjectName { get; set; }
        public string Level { get; set; }
        public string LearningGoal { get; set; }
        public string TutorRequirement { get; set; }
        public string Mode { get; set; }
        public string? ProvinceName { get; set; }
        public string? SubDistrictName { get; set; }
        public string? AddressLine { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateOnly? ExpectedStartDate { get; set; }
        public int ExpectedSessions { get; set; }
        public decimal? TargetUnitPriceMin { get; set; }
        public decimal? TargetUnitPriceMax { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt {  get; set; }
        public DateTime? ApprovedAt {  get; set; }
        public string? ApprovedBy {  get; set; }
        public string? RejectionReason {  get; set; }
        public string? CancelReason { get; set; }
        public List<ClassRequestSlotDto>? Slots { get; set; }
    }
}
