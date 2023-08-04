using System.Collections.Generic;

namespace nordelta.cobra.webapi.Configuration;

public class ReportQuotationsConfiguration
{
    public const string ReportToSGC = "ReportToSGC";
    
    public string Discriminator { get; set; }
    public List<string> RateTypes { get; set; }
}
