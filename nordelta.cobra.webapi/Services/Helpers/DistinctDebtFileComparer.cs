using nordelta.cobra.webapi.Models.ArchivoDeuda;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.Helpers
{
    public class DistinctDebtFileComparer: IEqualityComparer<ArchivoDeuda>
    {
        public bool Equals(ArchivoDeuda x, ArchivoDeuda y)
        {
            return x.Id == y.Id;
        }
        public int GetHashCode(ArchivoDeuda codeh)
        {
            return codeh.Id.GetHashCode();
        }
    }
}
