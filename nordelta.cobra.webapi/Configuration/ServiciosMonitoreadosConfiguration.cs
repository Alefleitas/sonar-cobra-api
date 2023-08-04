using System;

namespace nordelta.cobra.webapi.Configuration
{
    public class ServiciosMonitoreadosConfiguration
    {
        /// <summary>
        /// Esta propiedad no es un servicio representa el nombre de un sistema
        /// </summary>
        [Obsolete("Esta propiedad no es un servicio representa el nombre de un sistema")]
        public string HostCobraTemp { get; set; }
        public string CacUsd { get; set; } = "NOV_COBRA_CAC_USD";
        public string CacUsdCorp { get; set; } = "NOV_COBRA_CAC_USD_CORPO";
        public string Uva { get; set; } = "NOV_COBRA_UVA";
        public string UvaUsd { get; set; } = "NOV_COBRA_UVA_USD";
        public string PubDeudaCobra { get; set; } = "NOV_PUB_DEUDA_COBRA";
        public string PubDeudaGalicia { get; set; } = "NOV_PUB_DEUDA_COBRA_GALICIA";
        public string PubDeudaSantander { get; set; } = "NOV_PUB_DEUDA_COBRA_SANTANDER";
        public string ReportesOracle { get; set; } = "NOV_COBRA_REPORTES_ORACLE";
        public string TCMail { get; set; } = "NOV_COBRA_TC_MAIL";

    }
}
