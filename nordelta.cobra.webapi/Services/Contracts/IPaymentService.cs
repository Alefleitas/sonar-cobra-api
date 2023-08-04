using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IPaymentService
    {
        void InformPaymentDone(IOrderedEnumerable<DetalleDeuda> debtsWithSameCurrencyThanDebin, DateTime? fechaRecibo = null);
        List<ResumenCuentaResponse> GetPaymentsSummary(string cuit, string currentAccount);
        List<ResumenCuentaResponse> GetBalanceDetail(string cuit, string productCode);
        List<ResumenCuentaResponse> GetBalanceDetail(string cuit, string productCode, string clientReference = "");
        List<PaymentHistoryDto> GetApplicationDetail(string cuit, string productCode);
        List<PaymentHistoryDto> GetApplicationDetail(string cuit, string productCode, string clientReference = "");
        List<CuitProductBuDto> GetBalanceDetailProductList(string userCuit);
        List<CuitProductBuDto> GetApplicationDetailProductList(string userCuit);
        List<CuitProductBuDto> GetUnappliedProductList(string userCuit);
        Task<List<CuitProductBuDto>> GetApplicationDetailProductListAsync(string userCuit);
        Task<List<CuitProductBuDto>> GetBalanceDetailProductListAsync(string userCuit);
        Task<List<CuitProductBuDto>> GetUnappliedProductListAsync(string userCuit);
        List<CuitProductCurrencyDto> GetUnappliedProductList(List<string> userCuits);
        List<CuitProductCurrencyDto> GetDetailAndBalanceProductList(bool canViewAll, List<string> userCuits, User user = null);
        List<CuitProductCurrencyDto> GetDetailAndBalanceProductListAsync(string userCuit);
        List<CuitProductCurrencyDto> GetDetailAndBalanceProductListExternal(User user);
        List<PropertyCode> GetPropertyCodesForSummaryExternal(User user);
        List<PropertyCode> GetPropertyCodesForSummary(bool canViewAll, List<string> clientCuits, User user);
        List<PropertyCode> GetPropertyCodesForAdvance(bool canViewAll, List<string> clientCuits);
        Task<List<ApplicationDetailDto>> GetApplicationDetailAsync(ResumenCuentaRequest requestModel = null);
        Task<List<BalanceDetailDto>> GetBalanceDetailAsync(ResumenCuentaRequest request = null);
        List<BalanceDetailDto> GetRawBalanceDetail(string cuit, string productCode);
        List<SalesInvoiceAmountDto> GetSalesInvoiceAmount(List<PropertyCodeCuitDto> propertyCodes);
        List<DetalleDeuda> GetAllPayments(string cuit = "", string accountNumber = "");
        List<DetalleDeuda> GetAllPayments(List<string> cuits);
        List<PropertyCode> GetPropertyCodes(List<string> cuits = null, string accountNumber = null);
        List<PropertyCodeFull> GetPropertyCodesFull();
        List<DetalleDeuda> GetPaymentsByFFileName(string Cuit, string FFileName);
        List<ProductCodeBusinessUnitDTO> GetBusinessUnitByProductCodes(List<string> productCodes);
        Dictionary<string, string> GetBusinessUnitByProductCodeDictionary(List<string> productCodes);
        string GetReceipt(string buId, string docId, string legalEntityId);
        List<UnappliedPaymentDto> GetUnappliedPayments(string cuit, string productCode);
        List<UnappliedPaymentDto> GetUnappliedPayments(string cuit, string productCode, string clientReference = "");
        string GetInvoice(string trxId, string facElect);
        string UpdatePublishDebt(string cuit, string productCode, string publishDebt);
        Task<List<CuitProductCurrencyDto>> FetchDetailAndBalanceProductList();
        IEnumerable<string> GetBUListForCuits(List<string> cuits);
        List<DetalleDeuda> GetDebtsThatFitInMoneyAmount(IEnumerable<DetalleDeuda> debts, double moneyAmount);
        void InformPaymentMethodDone(PaymentMethod paymentMethod, int idClientOracle, int idSiteClientOracle, DateTime? fechaRecibo);
        List<LogDto> GetLogFromMiddleware(string queryParams);
        Task<List<DebtFreeNotificationDto>> GetDebtFreeForNotify();
        Task<string> UpdateNotificacionLibreDeuda(string cuit, string productCode);
    }
}
