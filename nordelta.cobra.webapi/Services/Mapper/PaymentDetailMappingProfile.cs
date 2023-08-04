using AutoMapper;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class PaymentDetailMappingProfile : Profile 
    {
        public PaymentDetailMappingProfile()
        {
            CreateMap<PaymentDetail, PaymentDetailDto>()
                .ForMember(dest => dest.Status, map => map.MapFrom(src => GetStatusDetail(src.PaymentMethod.Source, src.Status)))
                .ForMember(dest => dest.Instrument, map => map.MapFrom(src => GetInstrumentDetail(src.PaymentMethod.Source, src.Instrument)));
        }

        public string GetStatusDetail(PaymentSource source, string status)
        {
            return source switch
            {
                PaymentSource.Itau => GetStatusDetailItau(status),
                PaymentSource.Santander => status, 
                _ => string.Empty
            };
        }

        public string GetInstrumentDetail(PaymentSource source, string instrument)
        {
            return source switch
            {
                PaymentSource.Itau => GetInstrumentDetailItau(instrument),
                PaymentSource.Santander => instrument,
                _ => string.Empty
            };
        }

        public string GetStatusDetailItau(string status)
        {
            return status switch
            {
                ItauFilesConstants.ACREDITADO => nameof(ItauFilesConstants.ACREDITADO),
                ItauFilesConstants.CUSTODIA => nameof(ItauFilesConstants.CUSTODIA),
                ItauFilesConstants.DIFERIDO => nameof(ItauFilesConstants.DIFERIDO),
                ItauFilesConstants.DIFERIMIENTO_MANUAL => nameof(ItauFilesConstants.DIFERIMIENTO_MANUAL),
                ItauFilesConstants.DIFERIDO_AUTOMATICO => nameof(ItauFilesConstants.DIFERIDO_AUTOMATICO),
                ItauFilesConstants.REVERSADO => nameof(ItauFilesConstants.REVERSADO),
                ItauFilesConstants.INGRESADO => nameof(ItauFilesConstants.INGRESADO),
                ItauFilesConstants.PENDIENTE_DE_ACRED => nameof(ItauFilesConstants.PENDIENTE_DE_ACRED),
                ItauFilesConstants.RECHAZADO => nameof(ItauFilesConstants.RECHAZADO),
                ItauFilesConstants.RESCATADO => nameof(ItauFilesConstants.RESCATADO),
                ItauFilesConstants.ANULADO => nameof(ItauFilesConstants.ANULADO),
                ItauFilesConstants.PENDIENTE_DE_ACREDITACION_POR_REDEPOSITO => nameof(ItauFilesConstants.PENDIENTE_DE_ACREDITACION_POR_REDEPOSITO),
                _ => status
            };
        }

        public string GetInstrumentDetailItau(string instrument)
        {
            return instrument switch
            {
                ItauFilesConstants.EFECTIVO => nameof(ItauFilesConstants.EFECTIVO),
                ItauFilesConstants.DEB_EN_CTA => nameof(ItauFilesConstants.DEB_EN_CTA),
                ItauFilesConstants.DEBITO_CUENTA_OTRO_BCO => nameof(ItauFilesConstants.DEBITO_CUENTA_OTRO_BCO),
                ItauFilesConstants.CHEQUE_ITAU => nameof(ItauFilesConstants.CHEQUE_ITAU),
                ItauFilesConstants.CHEQUE_OTRO_BCO => nameof(ItauFilesConstants.CHEQUE_OTRO_BCO),
                ItauFilesConstants.CPD_ITAU => nameof(ItauFilesConstants.CPD_ITAU),
                ItauFilesConstants.CPD_OTRO_BCO => nameof(ItauFilesConstants.CPD_OTRO_BCO),
                ItauFilesConstants.CFU_ITAU => nameof(ItauFilesConstants.CFU_ITAU),
                ItauFilesConstants.CFU_OTRO_BCO => nameof(ItauFilesConstants.CFU_OTRO_BCO),
                ItauFilesConstants.CFU_CPD_ITAU => nameof(ItauFilesConstants.CFU_CPD_ITAU),
                ItauFilesConstants.CFU_CPD_OTRO_BCO => nameof(ItauFilesConstants.CFU_CPD_OTRO_BCO),
                ItauFilesConstants.DEBITO_DIRECTO_ITAU => nameof(ItauFilesConstants.DEBITO_DIRECTO_ITAU),
                ItauFilesConstants.DEBITO_DIRECTO_OTRO_BCO => nameof(ItauFilesConstants.DEBITO_DIRECTO_OTRO_BCO),
                ItauFilesConstants.TRANSFERENCIAS_OTRO_BCO => nameof(ItauFilesConstants.TRANSFERENCIAS_OTRO_BCO),
                ItauFilesConstants.ECHEQ_AL_DIA => nameof(ItauFilesConstants.ECHEQ_AL_DIA),
                ItauFilesConstants.ECHEQ_CPD => nameof(ItauFilesConstants.ECHEQ_CPD),
                _ => instrument
            };
        }
    }
}
