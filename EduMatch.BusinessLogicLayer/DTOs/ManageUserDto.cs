using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ManageUserDto
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Phone { get; set; }
        public DateTime CreateAt { get; set; }
        public bool? IsActive { get; set; }
        public List<string> Subjects { get; set; } = new();
        public decimal? HourlyRate { get; set; }
    }
}
