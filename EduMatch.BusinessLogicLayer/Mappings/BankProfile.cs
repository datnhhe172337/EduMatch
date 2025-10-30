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
    public class BankProfile : Profile
    {
        public BankProfile()
        {
            CreateMap<Bank, BankDto>();
        }
    }
}
