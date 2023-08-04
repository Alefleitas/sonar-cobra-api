using AutoMapper;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper;

public class DebtFreeNotificationMappingProfile : Profile
{
    public DebtFreeNotificationMappingProfile()
    {
        CreateMap<ReporteLibreDeDeudaResponse, DebtFreeNotificationDto>()
                .ForMember(dest => dest.Cuit, map => map.MapFrom(src => src.Cuit))
                .ForMember(dest => dest.RazonSocial, map => map.MapFrom(src => src.ClientName))
                .ForMember(dest => dest.Producto, map => map.MapFrom(src => src.Product));
    }
}
