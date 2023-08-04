namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles;

public class PsPaymentItau : FileRegistro
{
    public HeaderItau Header { get; set; }
    public PsRegistroCashIn Registro { get; set; }
}
