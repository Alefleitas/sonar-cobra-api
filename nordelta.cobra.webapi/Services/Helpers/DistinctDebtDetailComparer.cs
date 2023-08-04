using System;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Helpers
{
    public class DistinctDebtDetailComparer : IEqualityComparer<RepeatedDebtDetail>
    {
        public bool Equals(RepeatedDebtDetail x, RepeatedDebtDetail y)
        {
            return x.NroComprobante.Equals(y.NroComprobante) &&
                   x.FechaPrimerVenc.Equals(y.FechaPrimerVenc) &&
                   x.CodigoTransaccion.Equals(y.CodigoTransaccion) &&
                   x.CodigoProducto.Equals(y.CodigoProducto) &&
                   x.IdClienteOracle.Equals(y.IdClienteOracle) &&
                   x.IdSiteOracle.Equals(y.IdSiteOracle) &&
                   x.NroCuitCliente.Equals(y.NroCuitCliente) &&
                   x.RazonSocialCliente.Equals(y.RazonSocialCliente);
        }

        public int GetHashCode(RepeatedDebtDetail obj)
        {
            return obj.NroComprobante.GetHashCode() ^
                   obj.FechaPrimerVenc.GetHashCode() ^
                   obj.CodigoTransaccion.GetHashCode() ^
                   obj.CodigoProducto.GetHashCode() ^
                   obj.IdClienteOracle.GetHashCode() ^
                   obj.IdSiteOracle.GetHashCode() ^
                   obj.NroCuitCliente.GetHashCode() ^
                   obj.RazonSocialCliente.GetHashCode();

        }
    }
}
