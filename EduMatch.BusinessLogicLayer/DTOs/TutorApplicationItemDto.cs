using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class TutorApplicationItemDto
    {
        public int ApplicationId { get; set; }
        public int TutorId { get; set; }
        public string TutorName { get; set; }
        public string AvatarUrl { get; set; }
        public string Message { get; set; }
        //public int Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
