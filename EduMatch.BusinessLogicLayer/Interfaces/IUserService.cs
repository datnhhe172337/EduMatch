using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string email, string password, string baseUrl);
    }
}
