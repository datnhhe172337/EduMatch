using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ChatResponseDto
    {
        public int? SessionId { get; set; }
        public string Reply {  get; set; }

        public object Suggestions { get; set; }
    }
}
