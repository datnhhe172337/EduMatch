using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ChatRequestDto
    {
        public int? SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
