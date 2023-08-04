using nordelta.cobra.webapi.Models;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.Helpers
{
    public class DistinctDepartmentChangeNotificationComparer : IEqualityComparer<DepartmentChangeNotification>
    {
        public bool Equals(DepartmentChangeNotification x, DepartmentChangeNotification y)
        {
            return x.CodigoProducto.Equals(y.CodigoProducto) &&
                   x.NumeroCuitCliente.Equals(y.NumeroCuitCliente);
        }

        public int GetHashCode(DepartmentChangeNotification obj)
        {
            return obj.CodigoProducto.GetHashCode() ^
                   obj.NumeroCuitCliente.GetHashCode();

        }
    }
}
