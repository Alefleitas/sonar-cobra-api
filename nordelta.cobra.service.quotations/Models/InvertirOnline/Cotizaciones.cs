namespace nordelta.cobra.service.quotations.Models.InvertirOnline
{
    public class Cotizaciones
    {
        public Cotizaciones()
        {
            Titulos = new HashSet<CotizacionDetalle>();
        }

        public IEnumerable<CotizacionDetalle> Titulos { get; set; }
    }
}
