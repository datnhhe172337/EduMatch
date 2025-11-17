using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class UpdateTutorFeedbackRequest
    {
        public int BookingId { get; set; }
        public int TutorId { get; set; }
        public string? Comment { get; set; }
        public List<TutorFeedbackDetailDto> FeedbackDetails { get; set; } = new();
    }

}
