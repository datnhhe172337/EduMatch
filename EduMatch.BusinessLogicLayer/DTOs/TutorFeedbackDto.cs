using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class TutorFeedbackDto
    {
        public int Id { get; set; }
        public int TutorId { get; set; }
        public int BookingId { get; set; }
        public string LearnerEmail { get; set; } = string.Empty;
        public double OverallRating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public List<TutorFeedbackDetailDto> FeedbackDetails { get; set; } = new();
    }
    public class TutorFeedbackDetailDto
    {
        public int CriterionId { get; set; }
        public string? CriteriaName { get; set; }
        public int Rating { get; set; }
    }
}
