using System;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class HistoricQuotations
    {
        [Key]
        public int Id { get; set; }
        public ETipoQuote Tipo { get; set; }
        public ESubtipoQuote Subtipo { get; set; }
        public DateTime Fecha { get; set; }
        public string Titulo { get; set; }
        public double Valor { get; set; }
    }

}
