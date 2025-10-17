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
        public string? RoleName { get; set; }
        public string? Phone { get; set; }
        public DateTime CreateAt { get; set; }
        public bool? IsActive { get; set; }
        public List<string> Subjects { get; set; } = new();
        public List<decimal?> HourlyRate { get; set; } = new();
    }
}