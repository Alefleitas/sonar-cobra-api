namespace nordelta.cobra.webapi.Models;

public class ForeignCuit
{
    public string Cuit { get; set; }
    public TipoPersona TipoPersona { get; set; }
    public string Pais { get; set; }
}

public enum TipoPersona
{
    Juridica,    //0
    Fisica,      //1
    Otro         //2
}