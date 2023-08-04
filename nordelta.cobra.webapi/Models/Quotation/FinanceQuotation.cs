using System.Collections.Generic;

namespace nordelta.cobra.webapi.Models
{
    public class FinanceQuotation
    {
        public ETipoQuote Tipo { get; set; }
        public ESubtipoQuote Subtipo { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public double Valor { get; set; }
        public string Especie { get; set; }
        public TooltipMessage Tooltip { get; set; }

        public List<Variacion> Variacion { get; set; }

    }
    public enum ETipoQuote
    {
        DOLAR,
        ACCION,
        CAUCION,
        CANJE,
        INDICE,
    }
    public enum ESubtipoQuote
    {
        NINGUNO,
        PLAZO_CERCANO,
        PLAZO_LEJANO,
    }

    public class TooltipMessage
    {
        public string Tipo { get; set; }
        public string Mensaje { get; set; }
    }

    public class Variacion
    {
        public ETipoVariacion Tipo { get; set; }
        public double Valor { get; set; }
        public bool HistoricAvailable { get; set; }
    }

    public enum ETipoVariacion
    {
        DIARIA,
        MENSUAL,
        ANUAL
    }

    public static class TitulosQuote
    {
        public static string DolarMepCI = "Dólar MEP CI";
        public static string DolarCCLCI = "Dólar CCL CI";
        public static string DolarMep48Hs = "Dólar MEP 48 Horas";
        public static string DolarCCL48Hs = "Dólar CCL 48 Horas";
        public static string Canje = "Canje";
        public static string Merval = "Merval ARS";
        public static string MervalUsd = "Merval USD (CCL)";
        public static string DolarBlue = "Dólar Blue";
    }

}
