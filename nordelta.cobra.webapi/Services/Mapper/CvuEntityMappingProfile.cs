using AutoMapper;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class CvuEntityMappingProfile : Profile
    {
        public CvuEntityMappingProfile()
        {
            CreateMap<CvuEntityDto, CvuEntityViewModel>();
        }
    }
}
