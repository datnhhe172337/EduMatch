using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.DTOs;

namespace EduMatch.BusinessLogicLayer.Requests
{
    public class ClassCreateRequest
    {
        //[Required(ErrorMessage = "Learner email is required")]
        //[EmailAddress(ErrorMessage = "Invalid email format")]
        //public string LearnerEmail { get; set; } = string.Empty;

        [Required]
        public int SubjectId { get; set; }
        [Required]
        [StringLength(100)]
        public string? Title { get; set; }
        [Required]
        public int LevelId { get; set; }

        [Required]
        [StringLength(300)]
        public string LearningGoal { get; set; }
        [Required]
        [StringLength(500)]
        public string TutorRequirement { get; set; }
        [Required]
        public int Mode { get; set; }
        public int? ProvinceId { get; set; }
        public int? SubDistrictId { get; set; }
        public string? AddressLine { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        [Required]
        public DateOnly? ExpectedStartDate { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "ExpectedSessions must be between 1 and 100")]
        public int ExpectedSessions { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal? TargetUnitPriceMin { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal? TargetUnitPriceMax { get; set; }

        [Required]
        public List<ClassRequestSlotAvailabilityDto>? Slots { get; set; }
    }
}
