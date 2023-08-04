using nordelta.cobra.webapi.Services;
using System;
using Xunit;
using Moq;
using nordelta.cobra.webapi.Repositories;
using nordelta.cobra.webapi.Services.Contracts;
using System.Linq;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.NotificationModels.Delivery;
using nordelta.cobra.webapi.Repositories.Contexts;
using Microsoft.AspNetCore.Connections;
using SQLite;
using Microsoft.Data.Sqlite;
using DebitoInmediatoServiceItau;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Hangfire;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;

namespace nordelta.cobra.webapi.tests
{
    public class Notifications_Should
    {
        private INotificationService notificationService;
        private ICommunicationRepository communicationRepository;
        private RelationalDbContext context3;
        private InMemoryDbContext context2;
        private SqliteConnection connection3;
        private SqliteConnection connection2;
        private Mock<IMailService> mailServiceMock;
        private Mock<IPaymentService> paymentService;
        private InMemoryDbContext contextSsoEmpresas;
        private SqliteConnection connectionSsoEmpresas;
        private Mock<IConfiguration> configuration;
        private Mock<IRestClient> restClient;
        private Mock<IOptionsMonitor<ApiServicesConfig>> apiServicesConfig;
        private Mock<IMailService> mailService;
        private Mock<IExchangeRateFileRepository> exchangeRateFileRepository;
        private Mock<IOptionsMonitor<CustomItauCvuConfiguration>> customItauCvuConfig;
        private Mock<IBackgroundJobClient> _backgroundJobClient;

        private readonly string BU = "Consultatio S.A. - Puertos";

        private void ResetTest()
        {
            this.connection3 = TestDBHelper.GetOpenedConnection();
            this.connection2 = TestDBHelper.GetOpenedConnection();
            this.connectionSsoEmpresas = TestDBHelper.GetOpenedConnection();

            this.context3 = TestDBHelper.GetPopulatedRelationalContext(connection3, "CobraTestNotificationsDBData.sql");
            this.context2 = TestDBHelper.GetPopulatedInMemoryContext(connection2, "SsoUsersTestDBData.sql");
            this.contextSsoEmpresas = TestDBHelper.GetPopulatedInMemoryContext(connectionSsoEmpresas, "SsoEmpresasTestDBData.sql");


            var roleRepository = new RoleRepository(context3);
            var notificationRepository = new NotificationRepository(context3);
            var userRepository = new UserRepository(context2, roleRepository);
            var userChangesLogRepository = new UserChangesLogRepository(context3);
            this.communicationRepository = new CommunicationRepository(context3, userChangesLogRepository);

            var serviceProvider = new Mock<IServiceProvider>();

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            var archivoDeudaRepository = new ArchivoDeudaRepository(context3, userChangesLogRepository);
            var restrictionsListRepository = new RestrictionsListRepository(context3, userChangesLogRepository);
            this.customItauCvuConfig = new Mock<IOptionsMonitor<CustomItauCvuConfiguration>>();
            var accountBalanceRepository = new AccountBalanceRepository(context3, userRepository, userChangesLogRepository, serviceProvider.Object, customItauCvuConfig.Object);
            var empresaRepository = new EmpresaRepository(contextSsoEmpresas);
            this.configuration = new Mock<IConfiguration>();
            this.restClient = new Mock<IRestClient>();
            this.apiServicesConfig = new Mock<IOptionsMonitor<ApiServicesConfig>>();
            this.mailService = new Mock<IMailService>();
            this.exchangeRateFileRepository = new Mock<IExchangeRateFileRepository>();
            this._backgroundJobClient = new Mock<IBackgroundJobClient>();

            this.mailServiceMock = new Mock<IMailService>();
            mailServiceMock.Setup(x => x.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var userServiceMock = new Mock<IUserService>();

            var backgroundJobClient = new Mock<IBackgroundJobClient>();

            this.paymentService = new Mock<IPaymentService>();
            paymentService.Setup(x => x.GetBusinessUnitByProductCodes(It.IsAny<List<string>>())).Returns<List<string>>(a =>
            {
                return a.Select(x =>
                   {
                       var bupc = new ProductCodeBusinessUnitDTO();
                       bupc.Codigo = x;
                       bupc.BusinessUnit = BU;
                       return bupc;
                   }).ToList();
            });
            paymentService.Setup(x => x.GetBusinessUnitByProductCodeDictionary(It.IsAny<List<string>>())).Returns<List<string>>(a =>
            {
                var bupcDict = new Dictionary<string, string>();
                a.Distinct().ToList().ForEach(x =>
                {
                    bupcDict.Add(x, BU);
                });
                return bupcDict;
            });
            var users = context2.SsoUsers.ToList();
            var userRoles = context2.SsoUserRoles.ToList();
            foreach (var u in users)
            {
                var roles = userRoles.Where(x => x.UserId == u.IdApplicationUser);
                u.Roles = roles.Select(x => new SsoUserRole { Role = x.Role, UserId = u.IdApplicationUser }).ToList();
            }
            context2.SaveChanges();

            var deliveryTypeInjector = new DeliveryTypeInjector(mailServiceMock.Object);

            this.notificationService = new NotificationService(
                archivoDeudaRepository,
                userRepository,
                notificationRepository,
                deliveryTypeInjector,
                this.communicationRepository,
                accountBalanceRepository,
                empresaRepository,
                exchangeRateFileRepository.Object,
                paymentService.Object,
                configuration.Object,
                restClient.Object,
                apiServicesConfig.Object,
                mailService.Object,
                _backgroundJobClient.Object
            );
        }

        private void CloseConnections()
        {
            context3.Dispose();
            context2.Dispose();
            connection3.Close();
            connection2.Close();
        }

        [Theory]
        //Notificación de vencimientos del día
        //Vencimiento del día para el cuit 20183853792
        //Cantidad de vencimientos 2
        //Cantidad de emails a enviar 1
        //Cantidad de communications 2
        [InlineData(typeof(DayDue), 2019, 11, 19, 1, 1, 1)]

        public void CheckForDayDueNotifications(Type type, int year, int month, int day, int assertDayDueNotifsCount, int assertSentEmailsCount, int assertCommunicationsCount)
        {
            ResetTest();

            try
            {
                int initialCommunicationsCount = this.context3.Communications.ToList().Count;
                this.notificationService.CheckForNotifications(type, new DateTime(year, month, day));
                //Assert para cantidad de notificaciones
                Assert.Equal(this.context3.Notification.ToList().Count, assertDayDueNotifsCount);
                //Assert para cantidad de emails enviados
                this.mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(assertSentEmailsCount));
                //Assert para chequear que se hayan creado la misma cantidad de comunicaciones que de emails enviados
                Assert.Equal(this.context3.Communications.ToList().Count - initialCommunicationsCount, assertCommunicationsCount);
            }
            finally
            {
                CloseConnections();
            }
        }

        [Theory]
        //Notificación de vencimientos futuros
        //Vencimiento futuros para los cuits 20236698824 y 27927525408
        //Cantidad de vencimientos 2 (1 de cada 1)
        //Cantidad de emails a enviar 2 (1 a cada 1)
        [InlineData(typeof(FutureDue), 2020, 07, 31, 1, 2, 2)]

        public void CheckForFutureDueNotifications(Type type, int year, int month, int day, int assertDayDueNotifsCount, int assertSentEmailsCount, int assertCommunicationsCount)
        {
            ResetTest();

            try
            {
                int initialCommunicationsCount = this.context3.Communications.ToList().Count;
                this.notificationService.CheckForNotifications(type, new DateTime(year, month, day));
                //Assert para cantidad de notificaciones
                Assert.Equal(this.context3.Notification.ToList().Count, assertDayDueNotifsCount);
                //Assert para cantidad de emails enviados
                this.mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(assertSentEmailsCount));
                //Assert para chequear que se hayan creado la misma cantidad de comunicaciones que de emails enviados
                Assert.Equal(this.context3.Communications.ToList().Count - initialCommunicationsCount, assertCommunicationsCount);
            }
            finally
            {
                CloseConnections();
            }
        }

        [Theory]
        //Notificación de vencimientos pasados
        //Vencimiento futuros para los cuits 20183853792, 20236698824 y 27927525408
        //Cantidad de vencimientos 27 de 27927525408, 8 de 20236698824 y 9 de 20183853792
        //Cantidad de emails a enviar 3
        //Cantidad de communications 11
        [InlineData(typeof(PastDue), 2020, 07, 31, 1, 3, 11)]

        public void CheckForPastDueNotifications(Type type, int year, int month, int day, int assertDayDueNotifsCount, int assertSentEmailsCount, int assertCommunicationsCount)
        {
            ResetTest();

            try
            {
                int initialCommunicationsCount = this.context3.Communications.ToList().Count;
                this.notificationService.CheckForNotifications(type, new DateTime(year, month, day));
                //Assert para cantidad de notificaciones
                Assert.Equal(this.context3.Notification.ToList().Count, assertDayDueNotifsCount);
                //Assert para cantidad de emails enviados
                this.mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(assertSentEmailsCount));
                //Assert para chequear que se hayan creado la misma cantidad de comunicaciones que de emails enviados
                Assert.Equal(assertCommunicationsCount, this.context3.Communications.ToList().Count - initialCommunicationsCount);
            }
            finally
            {
                CloseConnections();
            }
        }

        //[Theory]
        //Notificación de debines
        //Hay 7 debines en total de los cuales hay ssoUser para 6 por ende se envían 6 mails
        //[InlineData(4, 6)]

        //public void CheckForDebinNotifications(int assertDebinNotifsCount, int assertSentEmailsCount)
        //{
        //    ResetTest();

        //    try
        //    {
        //        int initialCommunicationsCount = this.context3.Communications.ToList().Count;
        //        var debins = this.context3.Debin.Include(x => x.Debts).AsQueryable().ToList();
        //        this.notificationService.CheckForDebinNotifications(debins);
        //        //Assert para cantidad de notificaciones dependiendo los distintos estados de debin que hay en la BD
        //        var notificaciones = this.context3.Notification.ToList();
        //        Assert.Equal(notificaciones.Count, assertDebinNotifsCount);
        //        Assert.True(notificaciones.Exists(x => x.NotificationType.GetType() == typeof(DebinApproved)));
        //        Assert.True(notificaciones.Exists(x => x.NotificationType.GetType() == typeof(DebinRejected)));
        //        Assert.True(notificaciones.Exists(x => x.NotificationType.GetType() == typeof(DebinError)));
        //        Assert.True(notificaciones.Exists(x => x.NotificationType.GetType() == typeof(DebinExpired)));
        //        //Assert para cantidad de emails enviados
        //        this.mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(assertSentEmailsCount));
        //        //Assert para chequear que se hayan creado la misma cantidad de comunicaciones que de emails enviados
        //        Assert.Equal(this.context3.Communications.ToList().Count - initialCommunicationsCount, assertSentEmailsCount);
        //    }
        //    finally
        //    {
        //        CloseConnections();
        //    }
        //}

        [Theory]
        //Notificación de próximas comunicaciones
        //Notificaciones para los cuits 27927525408 y 20312023154
        //Cantidad de emails a enviar (1 para el cuit 20312023154, 0 para el cuit 27927525408 ya que no tiene rol de CuentasACobrar) Total 1
        [InlineData(typeof(NextCommunication), 2020, 07, 27, 1, 2)]

        public void CheckForNextCommunicationNotifications(Type type, int year, int month, int day, int assertNextCommunicationNotifsCount, int assertSentEmailsCount)
        {
            ResetTest();

            try
            {
                this.notificationService.CheckForNotifications(type, new DateTime(year, month, day));
                //Assert para cantidad de notificaciones
                Assert.Equal(this.context3.Notification.ToList().Count, assertNextCommunicationNotifsCount);
                //Assert para cantidad de emails enviados
                this.mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(assertSentEmailsCount));
            }
            finally
            {
                CloseConnections();
            }
        }

        [Theory]
        //Test de templates
        [InlineData(33, 1, 2723, 0, "<p>Estimado TEST_1:</p><p><br></p><p>El 19 agosto vence la cuota n° 0022 de su propiedad en Carpinchos  379 con n° codigo OPM0203S por un importe de&nbsp;107.486,94 ARS</p><p>&nbsp;</p><p>Ante cualquier inconveniente, comunicate con nosotros</p><p>&nbsp;</p><p>Saludamos atte.</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>NOTA: El capital en mora devengará un interés punitorio desde la fecha de vencimiento hasta efectivizar el pago.</p>")]
        [InlineData(27, 1, 3183, 4, "<p>Estimado TEST_1:</p><p><br></p><p>Se ha registrado un error en el procesamiento del débito inmediato con código 1CEB5821720147F5B19E509CD0156C18 por el pago de la cuota n°0022 en Carpinchos  223.</p><p><br></p><p>Para más información, comunicate con nosotros.</p><p>Saludamos atte.</p>")]
        [InlineData(34, 1, 3162, 0, @"<p>Estimado TEST_1:</p><p><br></p><p>Hoy vence la cuota n° 0018 de su propiedad en Carpinchos   20 por un importe de&nbsp;7.981,36&nbsp;USD. Recordamos que el capital en mora devengará un interés punitorio desde el día de la fecha hasta efectivizar el pago.</p><p>&nbsp;</p><p>Ante cualquier inconveniente, comunicate con nosotros</p><p>&nbsp;</p><p>Saludamos atte.</p><img src=""http://sitioweb.com.ar/image.jpg"">")]
        [InlineData(29, 3, 3174, 3, @"<p>Estimado TEST_3:</p><p><br></p><p>El débito inmediato con código FB5ECEDCB89940B49550C1456A84CF3E ha sido registrado con éxito en nuestra cuenta. Confirmamos el pago de la cuota n°0021 de la propiedad en Carpinchos   85</p><p><br></p><p>Por cualquier consulta, comunicate con nosotros.</p><p>Saludamos atte.</p><img src=""http://sitioweb.com.ar/image.jpg"">")]
        [InlineData(31, 3, 3174, 3, @"<p>Estimado TEST_3:</p><p><br></p><p>Su debin con código FB5ECEDCB89940B49550C1456A84CF3E ha sido rechazado.</p><p>El débito inmediato con código FB5ECEDCB89940B49550C1456A84CF3E por el pago de la cuota n°0021 en Carpinchos   85 ha sido rechazado.</p><p><span style=""color: red;""><span class=""ql-cursor"">﻿</span></span></p><p>Para más información, comunicate con nosotros a test@correo.com.</p><p>&nbsp;</p><p>Saludamos atte.</p>")]
        public void CheckTemplateBody(int notificationTypeId, int ssoUserId, int detalleDeudaId, int debinId, string body)
        {
            ResetTest();
            try
            {
                var notificationType = this.context3.NotificationType.Include(x => x.NotificationTypeRoles).ThenInclude(y => y.Role)
                .Include(x => x.Template)
                .Include(x => x.Delivery)
                .Include(x => x.Template)
                .Where(x => x.Id == notificationTypeId).Single();
                var ssoUser = this.context2.SsoUsers.Where(x => x.Id == ssoUserId).Single();
                var detalleDeuda = this.context3.DetallesDeuda.Where(x => x.Id == detalleDeudaId).SingleOrDefault();
                var debin = this.context3.Debin.Where(x => x.Id == debinId).SingleOrDefault();
                var templateReferences = this.context3.TemplateTokenReference.ToList();
                var detallesDeuda = this.context3.DetallesDeuda.ToList();
                List<string> productCodes = detallesDeuda.Select(x => x.ObsLibreCuarta.Trim()).Distinct().ToList();
                // Dictionary<string, string> businessUnitByProductCode = notificationService.GetBusinessUnitByProductCode(productCodes);
                Dictionary<string, string> businessUnitByProductCode = paymentService.Object.GetBusinessUnitByProductCodeDictionary(productCodes);
                var bodyhtml = this.notificationService.ReplaceTokens(notificationType.Template.HtmlBody, ssoUser, detalleDeuda, debin, null, templateReferences, businessUnitByProductCode, null);
                Assert.Equal(body, bodyhtml);
            }
            finally
            {
                CloseConnections();
            }
        }

        [Theory]
        //Test con template deshablitado
        //Cantidad de emails a enviar 0
        [InlineData(typeof(NextCommunication), 2020, 07, 27, 17, 0, 0)]

        public void CheckForNextCommunicationNotificationsWithDisabledTemplate(Type type, int year, int month, int day, int templateIdToToggle, int assertNextCommunicationNotifsCount, int assertSentEmailsCount)
        {
            ResetTest();

            try
            {
                this.communicationRepository.ToggleTemplate(templateIdToToggle);
                this.notificationService.CheckForNotifications(type, new DateTime(year, month, day));
                //Assert para cantidad de notificaciones
                Assert.Equal(this.context3.Notification.ToList().Count, assertNextCommunicationNotifsCount);
                //Assert para cantidad de emails enviados
                this.mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(assertSentEmailsCount));
            }
            finally
            {
                CloseConnections();
            }
        }
    }
}
