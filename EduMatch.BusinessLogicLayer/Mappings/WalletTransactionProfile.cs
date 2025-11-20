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
    public class WalletTransactionProfile : Profile
    {
        public WalletTransactionProfile()
        {
            CreateMap<WalletTransaction, WalletTransactionDto>()
                .ForMember(dest => dest.Booking, opt => opt.MapFrom(src => src.Booking));
        }
    }
}
