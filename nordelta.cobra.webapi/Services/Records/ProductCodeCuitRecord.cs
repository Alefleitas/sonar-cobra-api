using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Records;

public record ProductCodeCuitRecord(string Codigo, string Cuit, string ClientReference)
{
    public ProductCodeCuitRecord(ApplicationDetailSummaryDto applicationDetailSummary) 
        : this(applicationDetailSummary.Product, applicationDetailSummary.Cuit, applicationDetailSummary.ClientReference)
    { 
    }

    public ProductCodeCuitRecord(BalanceDetailSummaryDto balanceDetailSummary)
        : this(balanceDetailSummary.Product, balanceDetailSummary.Cuit, balanceDetailSummary.ClientReference)
    {
    }
}
