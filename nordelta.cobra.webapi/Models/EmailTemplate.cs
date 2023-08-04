using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    static class TemplateDescription
    {
        public const string
            Quotations = "QuotationBotTemplate",
            RejectedAdvanceeFee = "RejectedAdvanceFeeTemplate",
            FreeDebtUserReportTemplate = "FreeDebtUserReportTemplate",
            FreeDebtUser = "FreeDebtUserTemplate";
    }
}
