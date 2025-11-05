using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
    public class TutorApplicationEditRequest
    {
        [Required]
        public int TutorApplicationId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
