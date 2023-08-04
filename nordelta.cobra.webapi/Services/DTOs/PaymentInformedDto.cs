namespace nordelta.cobra.webapi.Services.DTOs;

public class PaymentInformedDto
{
    public int PaymentMethodId { get; set; }
    public PaymentInformedResult Result { get; set; }
    public string LockboxId { get; set; }
}

public enum PaymentInformedResult
{
    LockBox_Generado = 0,
    LockBox_No_Generado = 1,
}