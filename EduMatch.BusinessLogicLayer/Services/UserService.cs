using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;


        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<bool> RegisterAsync(string email, string password, string baseUrl)
        {
            if(await _userRepo.IsEmailAvailableAsync(email))
                return false;

            return true;
        }


    }
}
