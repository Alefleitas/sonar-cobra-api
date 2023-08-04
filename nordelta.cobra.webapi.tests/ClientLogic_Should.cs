using AutoFixture;
using Moq;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using nordelta.cobra.webapi.Services.Contracts;
using Xunit;
using static nordelta.cobra.webapi.Models.AccountBalance;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace nordelta.cobra.webapi.tests
{
    public class ClientLogic_Should
    {
        private Mock<IUserRepository> _userRepository;
        private Mock<IUserChangesLogRepository> _userChangesLogRepository;
        private Mock<IPaymentService> _paymentService;
        private IAccountBalanceRepository _accountBalanceRepository;
        private RelationalDbContext _context;
        private Mock<IOptionsMonitor<CustomItauCvuConfiguration>> _customItauCvuConfig;

        public ClientLogic_Should()
        {
            _userRepository = new Mock<IUserRepository>();
            _userChangesLogRepository = new Mock<IUserChangesLogRepository>();
            _paymentService = new Mock<IPaymentService>();
            _customItauCvuConfig = new Mock<IOptionsMonitor<CustomItauCvuConfiguration>>();

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

            serviceProvider
                .Setup(x => x.GetService(typeof(IPaymentService)))
                .Returns(_paymentService.Object);

            var options = new DbContextOptionsBuilder<RelationalDbContext>()
                .UseInMemoryDatabase(databaseName: $"CobraDbContext-{Guid.NewGuid()}")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new RelationalDbContext(options);

            _accountBalanceRepository = new AccountBalanceRepository(_context, _userRepository.Object,
                _userChangesLogRepository.Object, serviceProvider.Object, _customItauCvuConfig.Object);
        }

        [Fact]
        public void When_balance_changes_from_mora_to_aldia_department_must_be_cuentasxcobrar()
        {
            // Create Mock Data
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(NotificationType),
                    typeof(NextCommunication)));

            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(DeliveryType),
                    typeof(Email)));

            fixture.Customizations.Add(
            new TypeRelay(
                typeof(nordelta.cobra.webapi.Models.PaymentMethod),
                typeof(Debin)));

            const int accId = 134;

            var accFake = fixture.Build<AccountBalance>()
                .With(x => x.Id, accId)
                .With(x => x.Balance, EBalance.Mora)
                .With(x => x.Department, EDepartment.Legales)
                .Create();

            _context.AccountBalances.Add(accFake);
            _context.SaveChanges();

            // Test

            var acc = _accountBalanceRepository.GetAccountBalanceById(accId);
            acc.Balance = EBalance.AlDia;

            _accountBalanceRepository.InsertOrUpdate(new List<AccountBalance> { acc });

            var res = _accountBalanceRepository.GetAccountBalanceById(accId);
            Assert.Equal(EDepartment.CuentasACobrar, res.Department);
        }


        [Fact]
        public void When_previous_department_wasnt_external_and_now_is_publishDebt_must_be_false()
        {
            _paymentService.Setup(x => x.UpdatePublishDebt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("N");
            // Create Mock Data
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(NotificationType),
                    typeof(NextCommunication)));

            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(DeliveryType),
                    typeof(Email)));

            fixture.Customizations.Add(
            new TypeRelay(
                typeof(nordelta.cobra.webapi.Models.PaymentMethod),
                typeof(Debin)));

            const int accId = 139;

            var accFake = fixture.Build<AccountBalance>()
                .With(x => x.Id, accId)
                .With(x => x.Department, EDepartment.CuentasACobrar)
                .With(x => x.PublishDebt, "Y")
                .Create();

            _context.AccountBalances.Add(accFake);
            _context.SaveChanges();

            // Test

            var acc = _accountBalanceRepository.GetAccountBalanceById(accId);
            acc.Department = EDepartment.Externo;

            _accountBalanceRepository.InsertOrUpdate(new List<AccountBalance> { acc });

            var res = _accountBalanceRepository.GetAccountBalanceById(accId);
            Assert.Equal("N", res.PublishDebt);
        }

        [Fact]
        public void When_balance_changes_from_mora_to_aldia_contactstatus_must_be_nocontactado()
        {
            // Create Mock Data
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(NotificationType),
                    typeof(NextCommunication)));

            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(DeliveryType),
                    typeof(Email)));

            fixture.Customizations.Add(
            new TypeRelay(
                typeof(nordelta.cobra.webapi.Models.PaymentMethod),
                typeof(Debin)));

            const int accId = 144;

            var accFake = fixture.Build<AccountBalance>()
                .With(x => x.Id, accId)
                .With(x => x.Balance, EBalance.Mora)
                .With(x => x.ContactStatus, EContactStatus.Contactado)
                .Create();

            _context.AccountBalances.Add(accFake);
            _context.SaveChanges();

            // Test

            var acc = _accountBalanceRepository.GetAccountBalanceById(accId);
            acc.Balance = EBalance.AlDia;

            _accountBalanceRepository.InsertOrUpdate(new List<AccountBalance> { acc });

            var res = _accountBalanceRepository.GetAccountBalanceById(accId);
            Assert.Equal(EContactStatus.NoContactado, res.ContactStatus);
        }
    }
}
