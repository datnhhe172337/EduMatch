using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
    public class TutorApplyRequest
    {
        public int ClassRequestId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
