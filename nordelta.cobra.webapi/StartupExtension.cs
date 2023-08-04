using Microsoft.Extensions.DependencyInjection;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Models.MessageChannel;
using nordelta.cobra.webapi.Models.MessageChannel.Messages;
using nordelta.cobra.webapi.Models.NotificationModels.Delivery;
using nordelta.cobra.webapi.Repositories;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services;
using nordelta.cobra.webapi.Services.Contracts;
using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Services.Mocks;
using nordelta.cobra.webapi.Utils;
using Nordelta.Monitoreo.Nagios;
using RestSharp;
using System.Collections.Generic;
using nordelta.cobra.webapi.Websocket;
using nordelta.cobra.webapi.Models.ValueObject.Certificate;
using nordelta.cobra.webapi.Models.ValueObject.ItauPsp;

namespace nordelta.cobra.webapi
{
    public static class StartupExtension
    {
        public static IServiceCollection AddCobraConfigurations(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailConfig>(EmailConfig.EmailSmtpConfig, config.GetSection("ServiceConfiguration:EmailSmtp"))
                .Configure<EmailConfig>(EmailConfig.EmailSmtpConfig, x => x.Password = AesManager.GetPassword(x.Password, config.GetSection("SecretKeyUser").Value));

            services.Configure<EmailConfig>(EmailConfig.EmailSmtpQuotationBotConfig, config.GetSection("ServiceConfiguration:EmailSmtpQuotationBot"));

            services.Configure<EmailConfig>(EmailConfig.EmailImapQuotationBotConfig, config.GetSection("ServiceConfiguration:EmailImapQuotationBot"));

            services.Configure<AzureAdCredentialConfig>(AzureAdCredentialConfig.AzureAdQuotationBot, config.GetSection("ServiceConfiguration:AzureAdCredentialSettings"))
                .Configure<AzureAdCredentialConfig>(AzureAdCredentialConfig.AzureAdQuotationBot, x =>
                {
                    x.Password = AesManager.GetPassword(x.Password, config.GetSection("SecretKeyUser").Value);
                    x.ClientId = AesManager.GetPassword(x.ClientId, config.GetSection("SecretKeyCredential").Value);
                    x.TenantId = AesManager.GetPassword(x.TenantId, config.GetSection("SecretKeyCredential").Value);
                    x.ClientSecret = AesManager.GetPassword(x.ClientSecret, config.GetSection("SecretKeyCredential").Value);
                    x.RequesUri = x.RequesUri.Replace("TENANT_ID", x.TenantId);
                });

            services.Configure<ItauWCFConfiguration>(ItauWCFConfiguration.ArchivosCmlConfiguration,
                config.GetSection("ServiceConfiguration:ArchivosCmlConfiguration"));
            services.Configure<ItauWCFConfiguration>(ItauWCFConfiguration.DebitoInmediatoConfiguration,
                config.GetSection("ServiceConfiguration:DebitoInmediatoConfiguration"));
            services.Configure<ItauWCFConfiguration>(ItauWCFConfiguration.CuentaServiceConfiguration,
                config.GetSection("ServiceConfiguration:CuentaServiceConfiguration"));
            services.Configure<ItauWCFConfiguration>(ItauWCFConfiguration.ClienteServiceConfiguration,
                config.GetSection("ServiceConfiguration:ClienteServiceConfiguration"));

            services.Configure<TiposOperacionesConfiguration>(TiposOperacionesConfiguration.Aplicaciones, config.GetSection("TiposOperaciones:Aplicaciones"));
            services.Configure<TiposOperacionesConfiguration>(TiposOperacionesConfiguration.Saldos, config.GetSection("TiposOperaciones:Saldos"));

            services.Configure<ApiServicesConfig>(ApiServicesConfig.SgcApi, config.GetSection("ApiServices:SgcApi"));
            services.Configure<ApiServicesConfig>(ApiServicesConfig.SgfApi, config.GetSection("ApiServices:SgfApi"));
            services.Configure<ApiServicesConfig>(ApiServicesConfig.SsoApi, config.GetSection("ApiServices:SsoApi"));
            services.Configure<ApiServicesConfig>(ApiServicesConfig.CobraApi, config.GetSection("ApiServices:CobraApi"));
            services.Configure<ApiServicesConfig>(ApiServicesConfig.HolidaysApi, config.GetSection("ApiServices:HolidaysApi"));
            services.Configure<ApiServicesConfig>(ApiServicesConfig.ItauApi, config.GetSection("ApiServices:ItauApi"));
            services.Configure<ApiServicesConfig>(ApiServicesConfig.QuotationServiceApi, config.GetSection("ApiServices:QuotationServiceApi"));
            services.Configure<ApiServicesConfig>(ApiServicesConfig.MiddlewareApi, config.GetSection("ApiServices:MiddlewareApi"));

            services.Configure<CustomItauCvuConfiguration>(CustomItauCvuConfiguration.CustomItauCvuConfig, config.GetSection("CustomItauCvu"));

            services.Configure<ServiciosMonitoreadosConfiguration>(config.GetSection("Monitoreo:Servicios"));

            if (config.GetSection("PublishDebtFilesGalicia").Get<bool>() || config.GetSection("ProcessPaymentFilesOfGalicia").Get<bool>())
            {
                services.Configure<WinScpConfiguration>(WinScpConfiguration.GaliciaSftpConfig, config.GetSection("WinScpSettings:GaliciaSftp"))
                                .Configure<WinScpConfiguration>(WinScpConfiguration.GaliciaSftpConfig, option =>
                                {
                                    if (option.SessionOptions != null)
                                    {
                                        option.SessionOptions.Password = !string.IsNullOrEmpty(option.SessionOptions.Password) ?
                                        AesManager.GetPassword(option.SessionOptions.Password, config.GetSection("SecretKeyCertificate").Value) : string.Empty;

                                        option.SessionOptions.SshPrivateKey = !string.IsNullOrEmpty(option.SessionOptions.SshPrivateKey) ?
                                        AesManager.GetPassword(option.SessionOptions.SshPrivateKey, config.GetSection("SecretKeyCertificate").Value) : string.Empty;
                                    }
                                });
                services.Configure<List<CertificateConfiguration>>(CertificateConfiguration.GaliciaCertificates, config.GetSection("CertificateSettings:GaliciaCertificates"))
                    .Configure<List<CertificateConfiguration>>(CertificateConfiguration.GaliciaCertificates, option =>
                    {
                        option.ForEach(certificate =>
                        {
                            certificate.Password = string.IsNullOrEmpty(certificate.Password) ? certificate.Password : AesManager.GetPassword(certificate.Password, config.GetSection("SecretKeyCertificate").Value);
                        });
                    });
                services.Configure<List<PublishDebtConfiguration>>(PublishDebtConfiguration.GaliciaBUConfig, config.GetSection("PublishDebtSettings:GaliciaBusinessUnit"));
            }

            services.Configure<List<PublishDebtConfiguration>>(PublishDebtConfiguration.SantanderBUConfig, config.GetSection("PublishDebtSettings:SantanderBusinessUnit"));

            services.Configure<List<ReportQuotationsConfiguration>>(ReportQuotationsConfiguration.ReportToSGC, config.GetSection("ReportQuotationsConfiguration:ReportToSgc"));

            services.Configure<List<CertificateItem>>(CertificateItem.CertificateItems, config.GetSection("ServiceConfiguration:CertificateSettings:CertificateItems"))
                .Configure<List<CertificateItem>>(CertificateItem.CertificateItems, option => option.ForEach(certificateItem =>
                {
                    certificateItem.Password = AesManager.GetPassword(certificateItem.Password, config.GetSection("SecretKeyCertificate").Value);
                }));

            services.Configure<List<ItauPspItem>>(ItauPspItem.ItauPspItems, config.GetSection("ServiceConfiguration:CertificateSettings:ItauPSPIds"));

            services.AddNagiosMonitoring(config.GetSection("Monitoreo:NagiosConfig"));

            return services;
        }
        public static IServiceCollection AddCobraServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<IRestClient, RestClient>();
            //If DebugMode Enabled runs Mock, otherwise runs real service
            if (config.GetSection("ServiceConfiguration").GetSection("EnableItauMock").Get<bool>())
                services.AddTransient<IItauService, ItauServiceMock>();
            else
                services.AddTransient<IItauService, ItauService>();

            services.AddTransient<IDebinService, DebinService>();
            services.AddTransient<IAutomaticPaymentsService, AutomaticPaymentsService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<ILoginService, LoginService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IPaymentsFilesService, PaymentsFilesService>();
            services.AddTransient<IBankAccountService, BankAccountService>();
            services.AddTransient<IBankAccountFilesService, BankAccountFilesService>();
            services.AddTransient<IExchangeRateFilesService, ExchangeRateFilesService>();
            services.AddTransient<IPaymentHistoryService, PaymentHistoryService>();
            services.AddTransient<IAnonymousPaymentsService, AnonymousPaymentsService>();
            services.AddTransient<IAccountBalanceService, AccountBalanceService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<IContactDetailService, ContactDetailService>();
            services.AddTransient<IHolidaysService, HolidaysService>();
            services.AddTransient<IHelperService, HelperService>();
            services.AddTransient<IRestrictionsListService, RestrictionsListService>();

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IEmpresaService, EmpresaService>();
            services.AddTransient<IClientProfileService, ClientProfileService>();

            services.AddTransient<IMessageChannel<EmailMessage>, MailService>();
            

            services.AddTransient<IQuotationBotService, QuotationBotService>();
            services.AddTransient<IPaymentReportsService, PaymentReportsService>();

            services.AddTransient<ICvuEntityService, CvuEntityService>();
            services.AddTransient<IProcessTransactionService, ProcessTransactionService>();
            services.AddTransient<ICompanyService, CompanyService>();
            services.AddTransient<IItauArchivosService, ItauArchivosService>();

            services.AddTransient<ISantanderFilesService, SantanderFilesService>();
            services.AddTransient<IPaymentMethodService, PaymentMethodService>();
            services.AddTransient<IPaymentDetailService, PaymentDetailService>();

            services.AddTransient<IGaliciaFilesService, GaliciaFilesService>();
            services.AddTransient<IItauClienteService, ItauClienteService>();
            services.AddTransient<IValidacionClienteService, ValidacionClientesService>();

            services.AddSingleton<IFinanceQuotationsService, FinanceQuotationsService>();

            return services;
        }

        public static IServiceCollection AddCobraRepositories(this IServiceCollection services)
        {
            //// Repositories are injected as Scoped and services as Transient
            //// https://stackoverflow.com/a/38138494

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRepository<PublishedDebtFile>), typeof(Repository<PublishedDebtFile>));
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IDebinRepository, DebinRepository>();
            services.AddScoped<IAutomaticDebinRepository, AutomaticDebinRepository>();
            services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
            services.AddScoped<IArchivoDeudaRepository, ArchivoDeudaRepository>();
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IExchangeRateFileRepository, ExchangeRateFileRepository>();
            services.AddScoped<IAnonymousPaymentRepository, AnonymousPaymentRepository>();
            services.AddScoped<ICommunicationRepository, CommunicationRepository>();
            services.AddScoped<IContactDetailRepository, ContactDetailRepository>();
            services.AddScoped<IUserRepository, UserCacheRepository>();
            services.AddScoped<IEmpresaRepository, EmpresaCacheRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IHolidaysRepository, HolidaysRepository>();
            services.AddScoped<IDeliveryTypeInjector, DeliveryTypeInjector>();
            services.AddScoped<IRestrictionsListRepository, RestrictionsListRepository>();
            services.AddScoped<IUserChangesLogRepository, UserChangesLogRepository>();
            services.AddScoped<IPaymentReportRepository, PaymentReportsRepository>();
            services.AddScoped<ICvuEntityRepository, CvuEntityRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IPaymentDetailRepository, PaymentDetailRepository>();
            services.AddScoped<IHistoricQuotationsRepository, HistoricQuotationsRepository>();
            services.AddScoped<IPublishedDebtBankFileRepository, PublishedDebtBankFileRepository>();
            services.AddScoped<IPublishClientRepository, PublishClientRepository>();
            services.AddScoped<IValidacionClienteRepository, ValidacionClienteRepository>();
            services.AddScoped<IForeignCuitCacheRepository, ForeignCuitCacheRepository>();

            return services;
        }
    }
}
