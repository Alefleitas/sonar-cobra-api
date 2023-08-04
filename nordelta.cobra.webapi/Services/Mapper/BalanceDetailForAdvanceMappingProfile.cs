using AutoMapper;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class BalanceDetailForAdvanceMappingProfile : Profile
    {
        public BalanceDetailForAdvanceMappingProfile()
        {
            CreateMap<BalanceDetailForAdvanceResponse, ResumenCuentaResponse>().ReverseMap();

        }
    }
}
