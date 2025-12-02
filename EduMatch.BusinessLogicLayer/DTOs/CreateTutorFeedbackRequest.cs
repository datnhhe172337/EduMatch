using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class CreateTutorFeedbackRequest
    {
        public int BookingId { get; set; }
        public int TutorId { get; set; }
        public string? Comment { get; set; }
        public List<CreateTutorFeedbackDetailRequest> FeedbackDetails { get; set; } = new();
    }

    public class CreateTutorFeedbackDetailRequest
    {
        public int CriterionId { get; set; }
        public int Rating { get; set; } // 1–5
    }
}
