namespace nordelta.cobra.service.quotations.Models.OMS.Types
{
    public static class EntriesType
    {
        public const string
            UltimoPrecio = "LA",
            PrecioApertura = "OP",
            PrecioCierre = "CL",
            PrecioMaximo = "HI",
            PrecioMinimo = "LO",
            Moneda = "CC",
            PrecioCierreAnterior = "PC",
            Variacion = "VA",
            VolumenMonto = "VM",
            PrecioPromedioPonderadoPorVolumen = "VW"; // Volume Weighted Average Price
    }

    /*
     * OTROS *
    SE: Precio de liquidación (futuros)
    TV: Volumen operado
    PS:Precio liquidación anterior(Futuros)
    DI: dirección
    */
}