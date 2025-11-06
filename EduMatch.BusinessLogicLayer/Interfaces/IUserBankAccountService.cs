using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IUserBankAccountService
    {
        Task<IEnumerable<UserBankAccountDto>> GetAccountsByUserEmailAsync(string userEmail);
        Task<UserBankAccountDto> AddAccountAsync(AddUserBankAccountRequest request, string userEmail);
        Task<bool> RemoveAccountAsync(int accountId, string userEmail);
    }
}
