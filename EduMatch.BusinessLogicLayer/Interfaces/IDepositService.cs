using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IDepositService
    {
        Task<CreateDepositResponseDto> CreateDepositRequestAsync(CreateDepositRequest request, string userEmail);
    }
}
