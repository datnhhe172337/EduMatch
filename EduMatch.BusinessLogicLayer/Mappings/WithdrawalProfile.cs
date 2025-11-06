using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Mappings
{
    public class WithdrawalProfile : Profile
    {
        public WithdrawalProfile()
        {
            //for user
            CreateMap<Withdrawal, WithdrawalDto>()
                .ForMember(dest => dest.UserBankAccount, opt => opt.MapFrom(src => src.UserBankAccount));

            //for addmin
            CreateMap<Withdrawal, AdminWithdrawalDto>()
                .ForMember(dest => dest.UserBankAccount, opt => opt.MapFrom(src => src.UserBankAccount))
                .ForMember(dest => dest.Wallet, opt => opt.MapFrom(src => src.Wallet));
        }
    }
}
