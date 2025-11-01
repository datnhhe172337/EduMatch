using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ClassRequestSlotAvailabilityDto
    {
        public int DayOfWeek { get; set; } // 0=CN..6=T7
        public int SlotId { get; set; }
    }
}
