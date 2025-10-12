using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class TutorFilterDto
    {
        public string? Keyword { get; set; } 
        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? TeachingMode { get; set; } 
        public int? StatusId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
