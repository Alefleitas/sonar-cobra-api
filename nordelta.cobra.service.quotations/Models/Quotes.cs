using nordelta.cobra.service.quotations.Models.Enums;

namespace nordelta.cobra.service.quotations.Models
{
    public class Quotes : Quotation
    {
        public Quotes()
        {
            base.FromCurrency = "USD";
            base.ToCurrency = "ARS";
        }
        public ETipoQuote Tipo { get; set; }
        public ESubTipoQuote Subtipo { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Especie { get; set; }
        public TooltipMessage Tooltip { get; set; }
        public override double Calcular()
        {
            return base.Valor;
        }
    }

    public class TooltipMessage
    {
        public string Tipo { get; set; }
        public string Mensaje { get; set; }
    }
}
