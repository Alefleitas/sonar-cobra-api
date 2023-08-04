using AutoMapper;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;
using System;

namespace nordelta.cobra.webapi.Services.Mapper;

public class ValidacionClienteMappingProfile : Profile
{
    public ValidacionClienteMappingProfile()
    {
        CreateMap<ValidacionClientesDto, ValidacionCliente>();
    }
}
