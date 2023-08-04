using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models.MessageChannel;
using nordelta.cobra.webapi.Models.MessageChannel.Messages;
using nordelta.cobra.webapi.Repositories;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace nordelta.cobra.webapi.tests
{
    public class QuotationBotService_Should
    {
        private IConfiguration _configuration;
        private readonly Mock<IOptions<ServiciosMonitoreadosConfiguration>> _servicios;
        
        public QuotationBotService_Should() {
            _servicios = new Mock<IOptions<ServiciosMonitoreadosConfiguration>>();
        }

        private void SetAppSettings(string appSettingsName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Services"))
                .AddJsonFile(appSettingsName, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();

        }

        [Fact]
        public async Task DetectNewEmails()
        {
            var connection = TestDBHelper.GetOpenedConnection();
            var context = TestDBHelper.GetPopulatedRelationalContext(connection, "CobraTestBotQuotationDBData.sql");
            this.SetAppSettings("appsettings.json");

            var _emailConfig = new Mock<IOptionsMonitor<EmailConfig>>();
            var _azureAdCredentialConfig = new Mock<IOptionsMonitor<AzureAdCredentialConfig>>();

            _emailConfig.Setup(x => x.Get(EmailConfig.EmailSmtpQuotationBotConfig))
                .Returns(new EmailConfig());

            _emailConfig.Setup(x => x.Get(EmailConfig.EmailImapQuotationBotConfig))
                .Returns(new EmailConfig());

            _azureAdCredentialConfig.Setup(x => x.Get(AzureAdCredentialConfig.AzureAdQuotationBot))
                .Returns(new AzureAdCredentialConfig());

            var mailServicePartialMock = new Mock<MailService>(_emailConfig.Object, _azureAdCredentialConfig.Object, _configuration);

            var userChangesLogRepositoryMock = new Mock<IUserChangesLogRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            mailServicePartialMock.CallBase = true;

            mailServicePartialMock.Setup(x =>
                x.SendMessageAsync(It.IsAny<IMessage>())).Callback<IMessage>(message =>
                {
                    var msg = message.FullMessage();
                    Assert.True(msg == $@"Subject: RE: 
                        Body: La cotización de Dolar MEP vigente a este momento es de $142.79 (Calculado según AL30)");
                }
            );

            _servicios.Setup(x => x.Value).Returns(new ServiciosMonitoreadosConfiguration());

            var exchangeRateRepositoryMock = new Mock<ExchangeRateFileRepository>(context, null, userChangesLogRepositoryMock.Object, userRepositoryMock.Object);
            var notificationRepositoryMock = new Mock<NotificationRepository>(context);
            var quotationService = new QuotationBotService(exchangeRateRepositoryMock.Object, mailServicePartialMock.Object, notificationRepositoryMock.Object, _configuration, _servicios.Object);

            mailServicePartialMock.Setup(x => x.ListenForIncomingAsync(null, 0)).Callback(() =>
            {
                Task.Run(async () =>
                {
                    await quotationService.NotifyIncomingMessageAsync(
                        new EmailMessage { To = "testto@test.com", From = "testfrom@test.com" },
                        mailServicePartialMock.Object);
                });
            });

            mailServicePartialMock.Setup(x => x.SubscribeForIncoming(It.IsAny<IMessageChannelObserver>())).Verifiable();

            //This will endup firing the method with the Assert.True
            await quotationService.ListenAllChannelsAsync();
        }
    }
}
