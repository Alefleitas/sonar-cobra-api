namespace nordelta.cobra.service.quotations.Models.InvertirOnline
{
    public class CaucionTitulo
    {
        public int Plazo { get; set; }
        public double montoContado { get; set; }
        public double TasaPromedio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public TooltipMessage? Tooltip { get; set; }
    }
}
