using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordelta.cobra.service.quotations.Models.InvertirOnline.Endpoint
{
    public class ApiRoute
    {
        public const string Bonos = "/Cotizaciones/bonos/{0}/Argentina";
        public const string Cauciones = "Cotizaciones/cauciones/{0}/Argentina";
        public const string Cedears = "Cotizaciones/cedears/Argentina/{0}";
        public const string AccionesUSA = "Cotizaciones/acciones/estados_Unidos/{0}";
        public const string CotizacionDetalle = "{0}/Titulos/{1}/CotizacionDetalle";
        public const string CotizacionHistorica = "{0}/Titulos/{1}/Cotizacion/seriehistorica/{2}/{3}/{4}";
        ///{mercado}/Titulos/{simbolo}/Cotizacion/seriehistorica/{fechaDesde}/{fechaHasta}/{ajustada}
    }
}
