using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Mapper
{
    public class PaymentHistorTypeConverter : ITypeConverter<Debin, PaymentHistoryDto>
    {
        public PaymentHistoryDto Convert(Debin source, PaymentHistoryDto destination, ResolutionContext context)
        {
            var saldo = source.Debts.Where(x => int.Parse(x.CodigoMoneda) == (int) source.Currency).Sum(x =>
                            double.Parse(x.ImportePrimerVenc.Substring(0, x.ImportePrimerVenc.Length - 2))) -
                        source.Amount;
            var remaining = source.Amount;
            var dto = new PaymentHistoryDto();
            dto.FechaVenc = source.ExpirationDate.ToShortDateString();
            dto.Importe = source.Amount.ToString();
            dto.Moneda = source.Currency.ToString();
            dto.NroCuota = 0;
            dto.Saldo = saldo.ToString();
            dto.Details = new List<PaymentHistoryDetailDto>();
            dto.Producto = source.Debts.First().NroComprobante;
            foreach (var debt in source.Debts.Where(x => int.Parse(x.CodigoMoneda) == (int)source.Currency).OrderBy(x => x.FechaPrimerVenc))
            {
                var detail = new PaymentHistoryDetailDto();

                var importeCuota = double.Parse(debt.ImportePrimerVenc.Substring(0,
                    debt.ImportePrimerVenc.Length - 2));
                
                if (remaining >= importeCuota)
                {
                    detail.ImporteFC = importeCuota.ToString();
                    remaining = remaining - importeCuota;
                }
                else
                {
                    detail.ImporteFC = remaining.ToString();
                }
                detail.Fecha = debt.FechaPrimerVenc;
                detail.Moneda = debt.CodigoMoneda == "0" ? "ARS" : "USD";
                detail.Tipo = debt.TipoOperacion;
                detail.Importe = importeCuota.ToString();
                dto.Details.Add(detail);
            }
            return dto;
        }
    }
}
