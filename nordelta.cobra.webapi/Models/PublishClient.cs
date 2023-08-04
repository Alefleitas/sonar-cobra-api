using System;

namespace nordelta.cobra.webapi.Models;

public class PublishClient
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public string CuitBU { get; set; }
    public string Detail { get; set; }
    public EStatusPublishClient Status { get; set; }
    public PaymentSource Source { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? LastUpdatedOn { get; set; }
}

public enum EStatusPublishClient
{
    PUBLICADO = 0,
    NO_PUBLICADO = 1,
}