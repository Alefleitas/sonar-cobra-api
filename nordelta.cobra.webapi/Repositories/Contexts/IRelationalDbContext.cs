using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Repositories.Contexts
{
    public interface IRelationalDbContext : IUnitOfWork
    {
        DbSet<LockAdvancePayments> LockAdvancePayment { get; set; }
        DbSet<Restriction> RestrictionsList { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<Permission> PermissionsXRoles { get; set; }
        DbSet<AccountBalance> AccountBalances { get; set; }
        DbSet<Communication> Communications { get; set; }
        DbSet<ContactDetail> ContactDetails { get; set; }
        DbSet<BankAccount> BankAccounts { get; set; }
        DbSet<Bono> Bono { get; set; }
        DbSet<Debin> Debin { get; set; }
        DbSet<AutomaticPayment> AutomaticPayments { get; set; }
        DbSet<Company> Companies { get; set; }
        DbSet<ArchivoDeuda> ArchivosDeuda { get; set; }
        DbSet<HeaderDeuda> HeadersDeuda { get; set; }
        DbSet<DetalleDeuda> DetallesDeuda { get; set; }
        DbSet<TrailerDeuda> TrailersDeuda { get; set; }
        DbSet<OrganismoDeuda> OrganismosDeuda { get; set; }
        DbSet<ExchangeRateFile> ExchangeRateFile { get; set; }
        DbSet<AnonymousPayment> AnonymousPayment { get; set; }
        DbSet<Notification> Notification { get; set; }
        DbSet<NotificationType> NotificationType { get; set; }
        DbSet<NotificationTypeRole> NotificationTypeXRole { get; set; }
        DbSet<NotificationUser> NotificationXUser { get; set; }
        DbSet<DeliveryType> DeliveryType { get; set; }
        DbSet<Email> Email { get; set; }
        DbSet<Inbox> Inbox { get; set; }
        DbSet<FutureDue> FutureDue { get; set; }
        DbSet<DayDue> DayDue { get; set; }
        DbSet<PastDue> PastDue { get; set; }
        DbSet<DebinRejected> DebinRejected { get; set; }
        DbSet<DebinExpired> DebinExpired { get; set; }
        DbSet<DebinApproved> DebinApproved { get; set; }
        DbSet<DebinError> DebinError { get; set; }
        DbSet<DebinCancelled> DebinCancelled { get; set; }
        DbSet<NextCommunication> NextCommunication { get; set; }
        DbSet<Template> Template { get; set; }
        DbSet<TemplateTokenReference> TemplateTokenReference { get; set; }
        DbSet<Quotation> Quotations { get; set; }
        DbSet<DolarMEP> DolarMEP { get; set; }
        DbSet<UVA> UVA { get; set; }
        DbSet<CAC> CAC { get; set; }
        DbSet<CACUSD> CACUSD { get; set; }
        DbSet<CACUSDCorporate> CACUSDCorporate { get; set; }
        DbSet<UVAUSD> UVAUSD { get; set; }
        DbSet<BTC> BTC { get; set; }
        DbSet<BTCARS> BTCARS { get; set; }
        DbSet<ETH> ETH { get; set; }
        DbSet<ETHARS> ETHARS { get; set; }
        DbSet<USDTARS> USDTARS { get; set; }
        DbSet<USDTUSD> USDTUSD { get; set; }
        DbSet<QuotationExternal> QuotationExternal { get; set; }
        DbSet<PublishDebtRejectionFile> ArchivoDeudaRechazo { get; set; }
        DbSet<PublishDebtRejection> DetalleDeudaRechazo { get; set; }
        DbSet<PublishDebtRejectionError> DetalleDeudaRechazoError { get; set; }
        DbSet<RepeatedDebtDetail> RepeatedDebtDetail { get; set; }
        DbSet<DepartmentChangeNotification> DepartmentChangeNotifications { get; set; }
        DbSet<AdvanceFee> AdvanceFees { get; set; }
        DbSet<PublishedDebtFile> PublishedDebtFiles { get; set; }
        DbSet<UserChangesLog> UserChangesLog { get; set; }
        DbSet<USD> USD { get; set; }
        DbSet<EUR> EUR { get; set; }
        DbSet<USDUYU> USDUYU { get; set; }
        DbSet<ARSUYU> ARSUYU { get; set; }
        DbSet<EURUSD> EURUSD { get; set; }
        DbSet<PaymentReport> PaymentReports { get; set; }
        DbSet<CvuEntity> CvuEntities { get; set; }
        DbSet<CvuOperation> CvuOperations { get; set; }
        DbSet<Echeq> Echeq { get; set; }
        DbSet<PaymentMethod> PaymentMethod { get; set; }
        DbSet<Cash> Cash { get; set; }
        DbSet<Cheque> Cheque { get; set; }
        DbSet<PaymentDetail> PaymentDetail { get; set; }
        DbSet<PublishedDebtBankFile> PublishedDebtBankFile { get; set; }
        DbSet<DebtFree> DebtFree { get; set; }
    }
}