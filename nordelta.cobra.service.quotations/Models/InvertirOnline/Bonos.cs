namespace nordelta.cobra.service.quotations.Models.InvertirOnline
{
    public class Bonos
    {
        public Bonos()
        {
            Titulos = new HashSet<Titulo>();
        }
        public IEnumerable<Titulo> Titulos { get; set; }
    }
}
