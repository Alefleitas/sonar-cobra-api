namespace nordelta.cobra.webapi.Models
{
    public enum PaymentStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Expired = 3,
        Cancelled = 4,
        Error = 5,

        /// <summary>
        /// El pago fue procedo correctamente y esta listo para ser informado a Oracle (mediante SGF)
        /// <para>Nota: Debin no lo implementa</para>
        /// </summary>
        InProcess = 6,

        /// <summary>
        /// El pago fue informado correctamente a Oracle (mediante SGF)
        /// </summary>
        Finalized = 7,

        /// <summary>
        /// El pago no pudo ser informado de forma automatica por lo que se informo de forma manual a Oracle,
        /// es usado por los medios de pago que requieren ser informados por el cliente, ej: ECHEQ
        /// </summary>
        InformedManually = 8,

        /// <summary>
        /// El pago no pudo ser informado correctamente a Oracle (mediante SGF)
        /// <para>Caso 1: No se genero el archivo lockbox en SGF</para>
        /// <para>Caso 2: El middleware no informo correctamente el lockbox a Oracle</para> 
        /// </summary>
        ErrorInform = 9,

        /// <summary>
        /// El pago inicia un proceso cuyo final es que el mismo sea informado a Oracle (mediante SGF)
        /// </summary>
        Informing = 10
    }
}
