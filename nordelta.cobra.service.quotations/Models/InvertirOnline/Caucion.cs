namespace nordelta.cobra.service.quotations.Models.InvertirOnline
{
    public class Caucion
    {
        public Caucion()
        {
            Titulos = new HashSet<CaucionTitulo>();
        }
        public IEnumerable<CaucionTitulo> Titulos { get; set; }
    }
}
