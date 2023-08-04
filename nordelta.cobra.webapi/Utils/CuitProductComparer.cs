using System.Collections.Generic;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Utils
{
    public class CuitProductComparer : IEqualityComparer<CuitProductCurrencyDto>
    {
        public bool Equals(CuitProductCurrencyDto p1, CuitProductCurrencyDto p2)
        {
            return p1.Product == p2.Product && p1.Cuit == p2.Cuit && p1.ReferenciaCliente == p2.ReferenciaCliente;
        }

        public int GetHashCode(CuitProductCurrencyDto p)
        {
            return p.Product.GetHashCode() ^ p.Cuit.GetHashCode() ^ p.ReferenciaCliente.GetHashCode();
        }
    }
}
