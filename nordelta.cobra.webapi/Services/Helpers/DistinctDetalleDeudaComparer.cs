using System;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models.ArchivoDeuda;

namespace nordelta.cobra.webapi.Services.Helpers
{
    public class DistinctDetalleDeudaComparer : IEqualityComparer<DetalleDeuda>
    {
        public bool Equals(DetalleDeuda x, DetalleDeuda y)
        {
            return x.ArchivoDeudaId.Equals(y.ArchivoDeudaId) &&
                   x.NroComprobante.Equals(y.NroComprobante, StringComparison.OrdinalIgnoreCase) &&
                   x.FechaPrimerVenc.Equals(y.FechaPrimerVenc) &&
                   x.CodigoMoneda.Equals(y.CodigoMoneda) &&
                   x.ObsLibreSegunda.Equals(y.ObsLibreSegunda);
        }

        public int GetHashCode(DetalleDeuda obj)
        {
            return obj.ArchivoDeudaId.GetHashCode() ^
                   obj.NroComprobante.ToLowerInvariant().GetHashCode() ^
                   obj.FechaPrimerVenc.GetHashCode() ^
                   obj.CodigoMoneda.GetHashCode() ^
                   obj.ObsLibreSegunda.GetHashCode();

        }
    }
}
