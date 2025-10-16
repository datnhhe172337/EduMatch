using EduMatch.DataAccessLayer.Enum;
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
        public Gender? Gender { get; set; }
        public int? City { get; set; }
        public TeachingMode? TeachingMode { get; set; } 
        public TutorStatus? StatusId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
