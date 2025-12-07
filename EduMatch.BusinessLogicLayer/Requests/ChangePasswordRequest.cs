using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        public string oldPass {  get; set; } = string.Empty;
        [Required]
        public string newPass { get; set; }
    }
}
