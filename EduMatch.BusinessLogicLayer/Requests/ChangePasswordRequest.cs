using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
    public class ChangePasswordRequest
    {
        public string oldPass {  get; set; } = string.Empty;
        public string newPass { get; set; }
    }
}
