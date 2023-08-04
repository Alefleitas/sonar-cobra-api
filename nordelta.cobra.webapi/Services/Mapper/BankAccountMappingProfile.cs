using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class BankAccountMappingProfile : Profile
    {
        public BankAccountMappingProfile()
        {
            CreateMap<ValidateBankAccountResponse, ExternalValidateBankAccountResponse>()
                .ForMember(x => x.Moneda, y => y.MapFrom(x => x.Currency.ToString()));

            CreateMap<BankAccount, ExternalBankAccountViewModel>()
                .ForMember(x => x.Moneda, y => y.MapFrom(x => x.Currency.ToString()));
        }
    }
}
