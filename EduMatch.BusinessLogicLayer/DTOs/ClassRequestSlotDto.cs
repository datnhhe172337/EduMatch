using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ClassRequestSlotDto
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; } // 0-6
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
