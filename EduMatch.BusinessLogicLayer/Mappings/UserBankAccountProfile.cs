using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Bank;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Mappings
{
    public class UserBankAccountProfile : Profile
    {
        public UserBankAccountProfile()
        {
            CreateMap<UserBankAccount, UserBankAccountDto>()
                .ForMember(dest => dest.Bank, opt => opt.MapFrom(src => src.Bank));

            CreateMap<AddUserBankAccountRequest, UserBankAccount>(); // <-- ADD THIS
        }
    }
}
