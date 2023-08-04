using Newtonsoft.Json;

namespace nordelta.cobra.service.quotations.Models.InvertirOnline
{
    public class CotizacionDetalle
    {
        public CotizacionDetalle()
        {
            Puntas = new HashSet<Punta>();
        }

        [JsonProperty("ultimoPrecio")]

        public double UltimoPrecio { get; set; }

        [JsonProperty("variacionPorcentual")]
        public double VariacionPorcentual { get; set; }
        [JsonProperty("variacion")]

        public double Variacion { get; set; }


        [JsonProperty("apertura")]
        public double Apertura { get; set; }

        [JsonProperty("maximo")]
        public double Maximo { get; set; }

        [JsonProperty("minimo")]
        public double Minimo { get; set; }

        [JsonProperty("fechaHora")]
        public DateTimeOffset FechaHora { get; set; }

        [JsonProperty("tendencia")]
        public string Tendencia { get; set; }

        [JsonProperty("cierreAnterior")]
        public double CierreAnterior { get; set; }

        [JsonProperty("montoOperado")]
        public double MontoOperado { get; set; }

        [JsonProperty("volumenNominal")]
        public double VolumenNominal { get; set; }

        [JsonProperty("precioPromedio")]
        public double PrecioPromedio { get; set; }

        [JsonProperty("moneda")]
        public string Moneda { get; set; }

        [JsonProperty("precioAjuste")]
        public double PrecioAjuste { get; set; }

        [JsonProperty("interesesAbiertos")]
        public double InteresesAbiertos { get; set; }

        [JsonProperty("puntas")]
        public IEnumerable<Punta> Puntas { get; set; }

        [JsonProperty("cantidadOperaciones")]
        public double CantidadOperaciones { get; set; }

        [JsonProperty("simbolo")]
        public string Simbolo { get; set; }

        [JsonProperty("pais")]
        public string Pais { get; set; }

        [JsonProperty("mercado")]
        public string Mercado { get; set; }

        [JsonProperty("tipo")]
        public string Tipo { get; set; }

        [JsonProperty("descripcionTitulo")]
        public string DescripcionTitulo { get; set; }

        [JsonProperty("plazo")]
        public string Plazo { get; set; }

        [JsonProperty("laminaMinima")]
        public double LaminaMinima { get; set; }
        
        [JsonProperty("lote")]
        public double Lote { get; set; }

        [JsonProperty("cantidadMinima")]
        public long CantidadMinima { get; set; }

        [JsonProperty("puntosVariacion")]
        public double PuntosVariacion { get; set; }
    }
}
