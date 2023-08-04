using AutoMapper;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class PaymentReportMappingProfile : Profile
    {
        public PaymentReportMappingProfile()
        {
            CreateMap<PaymentReportViewModel, PaymentReportDto>();
        }
    }
}
