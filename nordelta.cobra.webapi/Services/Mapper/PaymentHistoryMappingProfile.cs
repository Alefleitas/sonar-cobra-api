using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class PaymentHistoryMappingProfile : Profile
    {
        public PaymentHistoryMappingProfile()
        {
            CreateMap<Debin, PaymentHistoryDto>().ConvertUsing<PaymentHistorTypeConverter>();
        }
    }
}
