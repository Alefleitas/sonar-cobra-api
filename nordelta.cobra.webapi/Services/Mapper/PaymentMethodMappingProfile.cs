using AutoMapper;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class PaymentMethodMappingProfile : Profile
    {
        public PaymentMethodMappingProfile()
        {
            CreateMap<PaymentMethod, PaymentMethodDto>()
                .ForMember(dest => dest.OlapAcuerdo, map => map.Ignore());
        }
    }
}
