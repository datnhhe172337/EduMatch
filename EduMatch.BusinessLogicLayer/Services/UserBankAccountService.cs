using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Bank;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserBankAccountService : IUserBankAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserBankAccountService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserBankAccountDto>> GetAccountsByUserEmailAsync(string userEmail)
        {
            var accounts = await _unitOfWork.UserBankAccounts.GetAccountsByUserEmailAsync(userEmail);
            return _mapper.Map<IEnumerable<UserBankAccountDto>>(accounts);
        }

        public async Task<UserBankAccountDto> AddAccountAsync(AddUserBankAccountRequest request, string userEmail)
        {
            var newAccount = _mapper.Map<UserBankAccount>(request);

            newAccount.UserEmail = userEmail;
            newAccount.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.UserBankAccounts.AddAsync(newAccount);
            await _unitOfWork.CompleteAsync();

            //fetch it again to include the Bank details
            var createdAccount = await _unitOfWork.UserBankAccounts.GetByIdAsync(newAccount.Id);
            return _mapper.Map<UserBankAccountDto>(createdAccount);
        }

        public async Task<bool> RemoveAccountAsync(int accountId, string userEmail)
        {
            var account = await _unitOfWork.UserBankAccounts.GetByIdAsync(accountId);

            if (account == null || account.UserEmail != userEmail)
            {
                return false;
            }
            _unitOfWork.UserBankAccounts.Remove(account);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }

}
