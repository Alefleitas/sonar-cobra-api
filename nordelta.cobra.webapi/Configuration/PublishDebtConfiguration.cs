using System.Collections.Generic;

namespace nordelta.cobra.webapi.Configuration
{
    public class PublishDebtConfiguration
    {
        public const string GaliciaBUConfig = "GaliciaBusinessUnit";
        public const string SantanderBUConfig = "SantanderBusinessUnit";

        public string Cuit { get; set; }
        public List<AcuerdoConfiguration> Acuerdos { get; set; }
    }

    public class AcuerdoConfiguration
    {
        public string CodigoOrganismo { get; set; }
        public int NroArchivo { get; set; }
        public string Currency { get; set; }
    }
}
