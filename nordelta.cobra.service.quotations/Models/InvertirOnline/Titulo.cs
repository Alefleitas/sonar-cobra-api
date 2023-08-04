namespace nordelta.cobra.service.quotations.Models.InvertirOnline
{
    public class Titulo
    {
        public string Simbolo { get; set; }
        public string Descripcion { get; set; }
        public string UltimoPrecio { get; set; }
        public DateTime Fecha { get; set; }
        public string Moneda { get; set; }
        public string Mercado { get; set; }
        public double VariacionPorcentual { get; set; }
    }
}
