using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;

namespace nordelta.cobra.webapi
{
    public static class StartupConfigHelper
    {
        public static void ConfigureHangfireRecurringJobs(IServiceProvider serviceProvider,
            RelationalDbContext relationalContext, IConfiguration config)
        {
            //Recurring tasks
            IPaymentsFilesService paymentsFilesService = serviceProvider.GetService<IPaymentsFilesService>();
            IBankAccountFilesService bankAccountFilesService = serviceProvider.GetService<IBankAccountFilesService>();
            IDebinService debinService = serviceProvider.GetService<IDebinService>();
            IAutomaticPaymentsService AutomaticPaymentsService = serviceProvider.GetService<IAutomaticPaymentsService>();
            IExchangeRateFilesService exchangeRateFilesService = serviceProvider.GetService<IExchangeRateFilesService>();
            IAnonymousPaymentsService AnonymousPaymentService = serviceProvider.GetService<IAnonymousPaymentsService>();
            IUserService userService = serviceProvider.GetService<IUserService>();
            IEmpresaService empresaService = serviceProvider.GetService<IEmpresaService>();
            INotificationService notificationService = serviceProvider.GetService<INotificationService>();
            IAccountBalanceService accountBalanceService = serviceProvider.GetService<IAccountBalanceService>();
            IHolidaysService holidaysService = serviceProvider.GetService<IHolidaysService>();
            IItauService itauService = serviceProvider.GetService<IItauService>();
            IPaymentService paymentService = serviceProvider.GetService<IPaymentService>();
            IHelperService helperService = serviceProvider.GetService<IHelperService>();
            IItauArchivosService itauArchivosService = serviceProvider.GetService<IItauArchivosService>();
            IPaymentMethodService paymentMethodService = serviceProvider.GetService<IPaymentMethodService>();
            ISantanderFilesService santanderFilesService = serviceProvider.GetService<ISantanderFilesService>();
            IGaliciaFilesService galiciaFilesService = serviceProvider.GetService<IGaliciaFilesService>();
            IValidacionClienteService validacionClienteService = serviceProvider.GetService<IValidacionClienteService>();
            IItauClienteService itauClienteService = serviceProvider.GetService<IItauClienteService>();
            ICvuEntityService cvuEntityService = serviceProvider.GetService<ICvuEntityService>();
            IPaymentReportsService paymentReportsService = serviceProvider.GetService<IPaymentReportsService>();

            TimeZoneInfo argentinaTimeZoneInfo = LocalDateTime.GetTimeZone("Argentina Standard Time");

            //Creates PaymentsFilesRejections folder if not exists
            paymentsFilesService.ProcessAllRejectionFiles();

            RecurringJob.AddOrUpdate(() => paymentsFilesService.ProcessAllFiles(), config.GetSection("RecurringJobs:ProcessPaymentsFilesJobCronExpression").Value, argentinaTimeZoneInfo, "files");

            RecurringJob.AddOrUpdate(() => paymentsFilesService.ProcessAllRejectionFiles(), config.GetSection("RecurringJobs:ProcessPublishDebtRejectionsJobCronExpression").Value, argentinaTimeZoneInfo, "rejectionfiles");

            RecurringJob.AddOrUpdate(() => exchangeRateFilesService.ProcessAllFiles(), config.GetSection("RecurringJobs:ProcessExchangeRateFilesJobCronExpression").Value, argentinaTimeZoneInfo);

            //This is just for initial data, would be ok if it is done only once.
            RecurringJob.AddOrUpdate(() => debinService.CheckEveryDebinStateAndSendRequestOnStatusChanged(null), config.GetSection("RecurringJobs:CheckDebinsStateJobCronExpression").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate(() => bankAccountFilesService.ProcessAllFiles(), config.GetSection("RecurringJobs:ProcessBankAccountsFilesJobCronExpression").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate(() => AutomaticPaymentsService.ExecutePaymentsFor(DateTime.Today), config.GetSection("RecurringJobs:AutomaticPaymentsJobCronExpression").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("syncSsoUsers", () => userService.SyncSsoUsers(), config.GetSection("RecurringJobs:UpdateSsoUsersJobCronExpression").Value, argentinaTimeZoneInfo, "sqlite");
            RecurringJob.Trigger("syncSsoUsers");

            RecurringJob.AddOrUpdate("syncSsoEmpresas", () => empresaService.SyncSsoEmpresas(), config.GetSection("RecurringJobs:UpdateSsoEmpresasJobCronExpression").Value, argentinaTimeZoneInfo);
            RecurringJob.Trigger("syncSsoEmpresas");

            RecurringJob.AddOrUpdate("syncHolidays", () => holidaysService.SyncHolidays(), config.GetSection("RecurringJobs:GetHolidaysJonCronExpression").Value, argentinaTimeZoneInfo);
            RecurringJob.Trigger("syncHolidays");

            var notificationTypes = relationalContext.NotificationType.Where(x => x.CronExpression != null).ToList();
            foreach (var notifType in notificationTypes)
                RecurringJob.AddOrUpdate("CheckForNotifications_" + notifType.GetType().Name,
                    () => notificationService.CheckForNotifications(notifType.GetType(),
                        TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, argentinaTimeZoneInfo)),
                    notifType.CronExpression, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("updateAccountBalances", () => accountBalanceService.UpdateAccountBalancesAsync(), config.GetSection("RecurringJobs:UpdateAccountBalancesJobCronExpression").Value, argentinaTimeZoneInfo);

            //Uses Scrapper at 7pm to get Dolar MEP value
            //RecurringJob.AddOrUpdate("getDolarMepValue", () => exchangeRateFilesService.GetDollarMep(), config.GetSection("RecurringJobs:GetDolarMEPQuotationJobCronExpression").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("getCacUsdValues", () => exchangeRateFilesService.GetUSDCAC(false),
                config.GetSection("RecurringJobs:GetCacUSDQuotationJobCronExpression").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("getUvaValues", () => exchangeRateFilesService.GetUVA_UVAUSD(),
                config.GetSection("RecurringJobs:GetUVAQuotationsJobCronExpression").Value, argentinaTimeZoneInfo);

            //Check
            RecurringJob.AddOrUpdate("sendRepeatedDebtDetailsEmail",
                () => paymentsFilesService.SendRepeatedDebtDetailsEmail(),
                config.GetSection("RecurringJobs:SendRepeatedDebtDetailsEmail").Value, argentinaTimeZoneInfo);
            // Uncomment when done on next stage
            //RecurringJob.AddOrUpdate(() => anonymousPaymentService.checkMigrationsForAnonymousPayments(DateTime.Today), Configuration.GetSection("RecurringJobs:ProcessAnonymousPaymentsMigrationJobCronExpression").Value);

            RecurringJob.AddOrUpdate("checkCertificateExpirationDate",
                () => itauService.CheckCertificateExpirationDate(), "0 7 * * *", argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("fetchProductList", () => paymentService.FetchDetailAndBalanceProductList(),
                config.GetSection("RecurringJobs:FetchDetailAndBalanceProductList").Value, argentinaTimeZoneInfo,
                "cache");
            RecurringJob.Trigger("fetchProductList");

            RecurringJob.AddOrUpdate(() => helperService.Healthy(),
                config.GetSection("RecurringJobs:ProcessKeepAliveCronExpression").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("sendDepartmentLegalesNotifications",
                () => accountBalanceService.SendRepeatedLegalEmail(),
                config.GetSection("RecurringJobs:SendRepeatedLegalEmail").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("SendPendingAdvanceFeeOrdersNotifications",
                () => notificationService.NotifyAdvanceFeeOrders(EAdvanceFeeStatus.Pendiente),
                config.GetSection("RecurringJobs:NotifyPendingAdvanceFeeOrdersJobCronExpression").Value,
                argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("SendApprovedAdvanceFeeOrdersNotifications",
                () => notificationService.NotifyAdvanceFeeOrders(EAdvanceFeeStatus.Aprobado),
                config.GetSection("RecurringJobs:NotifyApprovedAdvanceFeeOrdersJobCronExpression").Value,
                argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("NotifyTodayQuotations", () => notificationService.NotifyTodayQuotations(),
                config.GetSection("RecurringJobs:NotifyTodayQuotations").Value, argentinaTimeZoneInfo);

            // Itau
            RecurringJob.AddOrUpdate("GetAndProcessPaymentFilesOfItau", () => itauArchivosService.GetAndProcessPaymentFilesOfItau(), config.GetSection("RecurringJobs:GetAndProcessPaymentFilesOfItau").Value, argentinaTimeZoneInfo);

            // Check All Payment Methods Not Reported And Inform To Oracle
            RecurringJob.AddOrUpdate("InformAllPaymentMethodDone", () => paymentMethodService.InformAllPaymentMethodDone(), config.GetSection("RecurringJobs:InformAllPaymentMethodDone").Value, argentinaTimeZoneInfo);

            // Galicia
            if (config.GetSection("PublishDebtFilesGalicia").Get<bool>())
                RecurringJob.AddOrUpdate("CreateAndPublishDebtFilesToGalicia", () => galiciaFilesService.CreateAndPublishDebtFilesToGaliciaAsync(), config.GetSection("RecurringJobs:CreateAndPublishDebtFilesToGalicia").Value, argentinaTimeZoneInfo);
            else
                RecurringJob.RemoveIfExists("CreateAndPublishDebtFilesToGalicia");

            if (config.GetSection("ProcessPaymentFilesOfGalicia").Get<bool>())
                RecurringJob.AddOrUpdate("GetAndProcessPaymentFilesOfGalicia", () => galiciaFilesService.GetAndProcessPaymentFilesOfGaliciaAsync(), config.GetSection("RecurringJobs:GetAndProcessPaymentFilesOfGalicia").Value, argentinaTimeZoneInfo);
            else
                RecurringJob.RemoveIfExists("GetAndProcessPaymentFilesOfGalicia");

            // Santander
            if (config.GetSection("ProcessPaymentFilesOfSantander").Get<bool>())
                RecurringJob.AddOrUpdate("GetAndProcessPaymentFilesOfSantander", () => santanderFilesService.GetAndProcessPaymentFilesOfSantander(), config.GetSection("RecurringJobs:GetAndProcessPaymentFilesOfSantander").Value, argentinaTimeZoneInfo);
            else
                RecurringJob.RemoveIfExists("GetAndProcessPaymentFilesOfSantander");

            if (config.GetSection("PublishDebtFilesToSantander").Get<bool>())
                RecurringJob.AddOrUpdate("CreateAndPublishDebtFilesToSantander", () => santanderFilesService.CreateAndPublishDebtFilesToSantanderAsync(), config.GetSection("RecurringJobs:CreateAndPublishDebtFilesToSantander").Value, argentinaTimeZoneInfo);
            else
                RecurringJob.RemoveIfExists("CreateAndPublishDebtFilesToSantander");

            RecurringJob.AddOrUpdate("syncValidacionCliente", () => validacionClienteService.SyncValidacionCliente(), config.GetSection("RecurringJobs:SyncValidacionCliente").Value, argentinaTimeZoneInfo);
            RecurringJob.Trigger("syncValidacionCliente");
        
            // Test payment CVU
            //RecurringJob.AddOrUpdate("GetAndProcessPaymentFilesCVUOfItauLocal", () => itauArchivosService.GetAndProcessPaymentFilesCVUOfItauLocal(), config.GetSection("RecurringJobs:GetAndProcessPaymentFilesOfItau").Value, argentinaTimeZoneInfo);

            // Test payment ECHEQ
            //RecurringJob.AddOrUpdate("GetAndProcessPaymentFilesECHEQOfItauLocal", () => itauArchivosService.GetAndProcessPaymentFilesECHEQOfItauLocal(), config.GetSection("RecurringJobs:GetAndProcessPaymentFilesOfItau").Value, argentinaTimeZoneInfo);

            // Test payment Client Mass Publish
            //RecurringJob.AddOrUpdate("clientMassPublish", () => itauClienteService.ClientMassPublish(), config.GetSection("RecurringJobs:GetAndProcessPaymentFilesOfItau").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("checkAndFinalizePaymentInformed", () => paymentMethodService.CheckAndFinalizePaymentInformed(), config.GetSection("RecurringJobs:CheckAndFinalizePaymentInformed").Value, argentinaTimeZoneInfo);

            // CREACION MASIVA DE CVUs
            RecurringJob.AddOrUpdate("CvuMassCreationProcess", () => cvuEntityService.CvuMassCreationProcess(), config.GetSection("RecurringJobs:CvuMassCreationProcess").Value, argentinaTimeZoneInfo);

            // CREACION MASIVA DE CLIENTES ITAU
            RecurringJob.AddOrUpdate("MassPublicationOfItauClients", () => itauClienteService.ClientMassPublish(), config.GetSection("RecurringJobs:MassPublicationOfItauClients").Value, argentinaTimeZoneInfo);

            RecurringJob.AddOrUpdate("syncForeignCuits", () => userService.SyncForeignCuits(), config.GetSection("RecurringJobs:GetForeignCuitsJobCronExpression").Value, argentinaTimeZoneInfo, "sqlite");
            RecurringJob.Trigger("syncForeignCuits");

            RecurringJob.AddOrUpdate("syncRazonSocialOfAccountBalances", () => accountBalanceService.SyncRazonesSocialesAsync(), config.GetSection("RecurringJobs:SyncRazonSocialOfAccountBalancesJobCronExpression").Value, argentinaTimeZoneInfo);
        }

        public static void AddInitialDataRelationalContext(RelationalDbContext relationalContext, IConfiguration config)
        {
            Role someRole = relationalContext.Roles.FirstOrDefault();
            if (someRole == null)
            {
                relationalContext.Roles.AddRange(new List<Role>()
                {
                    new Role
                    {
                        Name = "Admin",
                        Description = "Can configure everything",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_Configuration },
                            new Permission { Code = EPermission.Access_Contact },
                            new Permission { Code = EPermission.Access_EverybodysPayments },
                            new Permission { Code = EPermission.Access_Payments },
                            new Permission { Code = EPermission.Access_MyAccountBalance },
                            new Permission { Code = EPermission.Access_CRM },
                            new Permission { Code = EPermission.Access_Support },
                            new Permission { Code = EPermission.Access_Quotations },
                            new Permission { Code = EPermission.Access_Rates },
                            new Permission { Code = EPermission.Access_Templates },
                            new Permission { Code = EPermission.Create_Quotation },
                            new Permission { Code = EPermission.Access_Automatic_Debt },
                            new Permission { Code = EPermission.Access_Debt_Post },
                            new Permission { Code = EPermission.Generate_Payment },
                            new Permission { Code = EPermission.Access_Reports },
                            new Permission { Code = EPermission.Inform_Manually },
                            new Permission { Code = EPermission.Access_Admin_AdvancePayments },
                            new Permission { Code = EPermission.Access_Admin_Payments },
                            new Permission { Code = EPermission.Access_Debt_Free }
                        }
                    },
                    new Role
                    {
                        Name = "CuentasACobrar",
                        Description = "Can look at Client's information and do basic configuration",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_Contact },
                            new Permission { Code = EPermission.Access_EverybodysPayments },
                            new Permission { Code = EPermission.Access_Payments },
                            new Permission { Code = EPermission.Access_MyAccountBalance },
                            new Permission { Code = EPermission.Access_CRM },
                            new Permission { Code = EPermission.Access_Quotations },
                            new Permission { Code = EPermission.Access_Rates },
                            new Permission { Code = EPermission.Access_Templates },
                            new Permission { Code = EPermission.Create_Quotation },
                            new Permission { Code = EPermission.Access_Automatic_Debt },
                            new Permission { Code = EPermission.Access_Debt_Post },
                            new Permission { Code = EPermission.Generate_Payment },
                            new Permission { Code = EPermission.Inform_Manually },
                            new Permission { Code = EPermission.Access_Admin_AdvancePayments },
                            new Permission { Code = EPermission.Access_Admin_Payments },
                            new Permission { Code = EPermission.Access_Debt_Free }
                        }
                    },
                    new Role
                    {
                        Name = "Cliente",
                        Description = "End user, for making payments",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_Payments },
                            new Permission { Code = EPermission.Access_MyAccountBalance },
                            new Permission { Code = EPermission.Access_Contact },
                            new Permission { Code = EPermission.Access_Rates },
                            new Permission { Code = EPermission.Access_FAQ },
                            new Permission { Code = EPermission.Access_Automatic_Debt },
                            new Permission { Code = EPermission.Generate_Payment },
                            new Permission { Code = EPermission.Access_AdvancePayments }
                        }
                    },
                    new Role
                    {
                        Name = "Soporte",
                        Description = "Support user, can access as client user and act on its behalf",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_Support },
                            new Permission { Code = EPermission.Access_Rates }
                        }
                    },
                    new Role
                    {
                        Name = "Legales",
                        Description = "Can look at Client's information",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_CRM },
                            new Permission { Code = EPermission.Access_Rates }
                        }
                    },
                    new Role
                    {
                        Name = "Criba",
                        Description = "Can look at Criba Client's information",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_CRM },
                            new Permission { Code = EPermission.Access_Rates }
                        }
                    },
                    new Role
                    {
                        Name = "Externo",
                        Description = "Can only look at Client's account balances that belong to this department",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_CRM },
                            new Permission { Code = EPermission.Access_Rates }
                        }
                    },
                    new Role
                    {
                        Name = "Comercial",
                        Description = "Can only look at Client's information and give support",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_Support },
                            new Permission { Code = EPermission.Access_EverybodysPayments },
                            new Permission { Code = EPermission.Access_Payments },
                            new Permission { Code = EPermission.Access_MyAccountBalance },
                            new Permission { Code = EPermission.Access_CRM },
                            new Permission { Code = EPermission.Access_Quotations },
                            new Permission { Code = EPermission.Access_Rates }
                        }
                    },
                    new Role
                    {
                        Name = "AtencionAlCliente",
                        Description = "Can only look at Client's account balances and give support",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_CRM },
                            new Permission { Code = EPermission.Access_Rates },
                            new Permission { Code = EPermission.Access_Support }
                        }
                    },
                    new Role
                    {
                        Name = "ObrasParticulares",
                        Description = "Can only look at Client's account balances and edit work started",
                        Permissions = new List<Permission>()
                        {
                            new Permission { Code = EPermission.Access_CRM }
                        }
                    }
                });
            }

            Company someCompany = relationalContext.Companies.FirstOrDefaultAsync().Result;
            if (someCompany == null)
            {
                relationalContext.Companies.AddRange(new List<Company>()
                {
                    new Company
                    {
                        Cuit = "30709054038",
                        CbuPeso = "2590033210318110510087",
                        CbuDolar = "2590033211318110560151",
                        SocialReason = "Fideicomiso Golf Club Nordelta",
                        ConvenioPeso = "000001",
                        ConvenioDolar = "000002"
                    },
                    new Company
                    {
                        Cuit = "30658660892",
                        CbuPeso = "2590033210317988010031",
                        CbuDolar = "2590033211317988060105",
                        SocialReason = "Nordelta SA",
                        ConvenioPeso = "000001",
                        ConvenioDolar = "000002"
                    },
                    new Company
                    {
                        Cuit = "30587480359",
                        CbuPeso = "2590033210318072110046",
                        CbuDolar = "2590033211318072160110",
                        SocialReason = "Consultatio SA (Puertos)",
                        ConvenioPeso = "000001",
                        ConvenioDolar = "000002"
                    },
                    new Company
                    {
                        Cuit = "30715720902",
                        CbuPeso = "2590051610343397500100",
                        CbuDolar = "2590051611344356060177",
                        SocialReason = "UTE Puerto Madero",
                        ConvenioPeso = "000001",
                        ConvenioDolar = "000003"
                    },
                });
            }

            TemplateTokenReference templateTokenReference =
                relationalContext.TemplateTokenReference.FirstOrDefaultAsync().Result;
            if (templateTokenReference == null)
            {
                relationalContext.TemplateTokenReference.AddRange(
                    new TemplateTokenReference
                    {
                        Description = "Nombre de la propiedad",
                        Token = "{{PROPIEDAD_NOMBRE}}",
                        ObjectProperty = "NroComprobante"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Nombre y Apellido del Cliente",
                        Token = "{{CLIENTE_NOMBRE}}",
                        ObjectProperty = "RazonSocial"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Cuit del Cliente",
                        Token = "{{CLIENTE_CUIT}}",
                        ObjectProperty = "Cuit"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Código del Debin",
                        Token = "{{DEBIN_CODIGO}}",
                        ObjectProperty = "DebinCode"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Valor de la cuota",
                        Token = "{{CUOTA_VALOR}}",
                        ObjectProperty = "ImportePrimerVenc"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Moneda del a cuota",
                        Token = "{{CUOTA_MONEDA}}",
                        ObjectProperty = "CodigoMoneda"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Fecha de vencimiento de cuota",
                        Token = "{{CUOTA_VENCIMIENTO}}",
                        ObjectProperty = "FechaPrimerVenc"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Número de cuota",
                        Token = "{{CUOTA_NRO}}",
                        ObjectProperty = "NroCuota"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Código de producto",
                        Token = "{{CODIGO_PRODUCTO}}",
                        ObjectProperty = "CodProducto"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Firma correspondiente a la BU",
                        Token = "{{FIRMA_BU}}",
                        ObjectProperty = "FirmaBU"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Correo correspondiente a la BU",
                        Token = "{{CORREO_BU}}",
                        ObjectProperty = "CorreoBU"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Tabla detalle de deudas",
                        Token = "{{TABLA_DETALLE_DEUDA}}",
                        ObjectProperty = "TablaDetalleDeuda"
                    },
                    new TemplateTokenReference
                    {
                        Description = "Nombre de la BU",
                        Token = "{{NOMBRE_EMPRESA}}",
                        ObjectProperty = "NombreEmpresa"
                    }
                );
            }
            else
            {
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{PROPIEDAD_NOMBRE}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Nombre de la propiedad",
                        Token = "{{PROPIEDAD_NOMBRE}}",
                        ObjectProperty = "NroComprobante"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CLIENTE_NOMBRE}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Nombre y Apellido del Cliente",
                        Token = "{{CLIENTE_NOMBRE}}",
                        ObjectProperty = "RazonSocial"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CLIENTE_CUIT}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Cuit del Cliente",
                        Token = "{{CLIENTE_CUIT}}",
                        ObjectProperty = "Cuit"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{DEBIN_CODIGO}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Código del Debin",
                        Token = "{{DEBIN_CODIGO}}",
                        ObjectProperty = "DebinCode"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CUOTA_VALOR}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Valor de la cuota",
                        Token = "{{CUOTA_VALOR}}",
                        ObjectProperty = "ImportePrimerVenc"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CUOTA_MONEDA}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Moneda del a cuota",
                        Token = "{{CUOTA_MONEDA}}",
                        ObjectProperty = "CodigoMoneda"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CUOTA_VENCIMIENTO}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Fecha de vencimiento de cuota",
                        Token = "{{CUOTA_VENCIMIENTO}}",
                        ObjectProperty = "FechaPrimerVenc"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CUOTA_NRO}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Número de cuota",
                        Token = "{{CUOTA_NRO}}",
                        ObjectProperty = "NroCuota"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CODIGO_PRODUCTO}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Código de producto",
                        Token = "{{CODIGO_PRODUCTO}}",
                        ObjectProperty = "CodProducto"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{FIRMA_BU}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Firma correspondiente a la BU",
                        Token = "{{FIRMA_BU}}",
                        ObjectProperty = "FirmaBU"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{CORREO_BU}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Correo correspondiente a la BU",
                        Token = "{{CORREO_BU}}",
                        ObjectProperty = "CorreoBU"
                    });
                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{TABLA_DETALLE_DEUDA}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Tabla detalle de deudas",
                        Token = "{{TABLA_DETALLE_DEUDA}}",
                        ObjectProperty = "TablaDetalleDeuda",
                    });

                if (!relationalContext.TemplateTokenReference.Any(it => it.Token == "{{NOMBRE_EMPRESA}}"))
                    relationalContext.TemplateTokenReference.Add(new TemplateTokenReference
                    {
                        Description = "Nombre de la BU",
                        Token = "{{NOMBRE_EMPRESA}}",
                        ObjectProperty = "NombreEmpresa"
                    });

            }

            LockAdvancePayments lockAdvancePayments = relationalContext.LockAdvancePayment.FirstOrDefaultAsync().Result;
            if (lockAdvancePayments == null)
            {
                relationalContext.LockAdvancePayment.Add(new LockAdvancePayments
                {
                    LockedByUser = false,
                });
            }

            //Temporary Save
            relationalContext.SaveChanges();

            NotificationType someNotificationType = relationalContext.NotificationType.FirstOrDefaultAsync().Result;
            if (someNotificationType == null)
            {
                var deliveryTypeForNotificationTypes = new Email()
                {
                    Configuration = "testing@novit.com.ar"
                };
                relationalContext.NotificationType.AddRange(new List<NotificationType>()
                {
                    new DayDue
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por vencimiento de cuotas en el día",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList(),
                        CronExpression = "0 10 * * *"
                    },
                    new FutureDue
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = String.Format(@"Alerta por cuotas a vencer en {0} días",
                            config.GetSection("DaysOfNotificationForFutureDue").Value),
                        ConfigurationDays = Convert.ToInt32(config.GetSection("DaysOfNotificationForFutureDue").Value),
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList(),
                        CronExpression = "0 10 * * *"
                    },
                    new PastDue
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = String.Format(@"Alerta por cuotas vencidas en los últimos {0} días",
                            config.GetSection("DaysOfNotificationForPastDue").Value),
                        ConfigurationDays = Convert.ToInt32(config.GetSection("DaysOfNotificationForPastDue").Value),
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList(),
                        CronExpression = "0 10 * * *"
                    },
                    new DebinRejected
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por Debin Rechazado",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList()
                    },
                    new DebinExpired
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por Debin Expirado",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList()
                    },
                    new DebinApproved
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por Debin Aprobado",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList()
                    },
                    new DebinCancelled
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por Debin Cancelado",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList()
                    },
                    new DebinError
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por Debin Error",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "Cliente" || x.Name == "CuentasACobrar")
                            .ToList()
                    },
                    new NextCommunication
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por comunicaciones próximas",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "CuentasACobrar").ToList(),
                        CronExpression = "0 8 * * *"
                    },
                    new DebtFree
                    {
                        Delivery = deliveryTypeForNotificationTypes,
                        Description = "Alerta por libre deuda",
                        ConfigurationDays = 0,
                        Roles = relationalContext.Roles.Where(x => x.Name == "CuentasACobrar" || x.Name == "Admin").ToList(),
                        CronExpression = null
                    }
                });
            }

            relationalContext.SaveChanges();
        }
    }
}
