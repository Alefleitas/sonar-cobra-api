using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using nordelta.cobra.webapi.Helpers;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using Microsoft.EntityFrameworkCore.Storage;

namespace nordelta.cobra.webapi.Repositories.Contexts
{
    public partial class RelationalDbContext : DbContext, IRelationalDbContext
    {
        public RelationalDbContext(DbContextOptions<RelationalDbContext> options)
            : base(options)
        { }

        public DbSet<LockAdvancePayments> LockAdvancePayment { get; set; }
        public DbSet<Restriction> RestrictionsList { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> PermissionsXRoles { get; set; }
        public DbSet<AccountBalance> AccountBalances { get; set; }
        public DbSet<Communication> Communications { get; set; }
        public DbSet<ContactDetail> ContactDetails { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Bono> Bono { get; set; }
        //public DbSet<Client> Clients { get; set; }
        public DbSet<Debin> Debin { get; set; }
        public DbSet<AutomaticPayment> AutomaticPayments { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ArchivoDeuda> ArchivosDeuda { get; set; }
        public DbSet<HeaderDeuda> HeadersDeuda { get; set; }
        public DbSet<DetalleDeuda> DetallesDeuda { get; set; }
        public DbSet<TrailerDeuda> TrailersDeuda { get; set; }
        public DbSet<OrganismoDeuda> OrganismosDeuda { get; set; }
        public DbSet<ExchangeRateFile> ExchangeRateFile { get; set; }
        public DbSet<AnonymousPayment> AnonymousPayment { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationType> NotificationType { get; set; }
        public DbSet<NotificationTypeRole> NotificationTypeXRole { get; set; }
        public DbSet<NotificationUser> NotificationXUser { get; set; }
        public DbSet<DeliveryType> DeliveryType { get; set; }
        public DbSet<Email> Email { get; set; }
        public DbSet<Inbox> Inbox { get; set; }
        public DbSet<FutureDue> FutureDue { get; set; }
        public DbSet<DayDue> DayDue { get; set; }
        public DbSet<PastDue> PastDue { get; set; }
        public DbSet<DebinRejected> DebinRejected { get; set; }
        public DbSet<DebinExpired> DebinExpired { get; set; }
        public DbSet<DebinApproved> DebinApproved { get; set; }
        public DbSet<DebinError> DebinError { get; set; }
        public DbSet<DebinCancelled> DebinCancelled { get; set; }
        public DbSet<NextCommunication> NextCommunication { get; set; }
        public DbSet<Template> Template { get; set; }
        public DbSet<TemplateTokenReference> TemplateTokenReference { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<DolarMEP> DolarMEP { get; set; }
        public DbSet<UVA> UVA { get; set; }
        public DbSet<CAC> CAC { get; set; }
        public DbSet<CACUSD> CACUSD { get; set; }
        public DbSet<CACUSDCorporate> CACUSDCorporate { get; set; }
        public DbSet<UVAUSD> UVAUSD { get; set; }
        public DbSet<BTC> BTC { get; set; }
        public DbSet<BTCARS> BTCARS { get; set; }
         public DbSet<ETH> ETH { get; set; }
        public DbSet<ETHARS> ETHARS { get; set; }
        public DbSet<USDTARS> USDTARS { get; set; }
        public DbSet<USDTUSD> USDTUSD { get; set; }
        public DbSet<QuotationExternal> QuotationExternal { get; set; }
        public DbSet<PublishDebtRejectionFile> ArchivoDeudaRechazo {get; set;}
        public DbSet<PublishDebtRejection> DetalleDeudaRechazo { get; set; }
        public DbSet<PublishDebtRejectionError> DetalleDeudaRechazoError { get; set; }
        public DbSet<RepeatedDebtDetail> RepeatedDebtDetail { get; set; }
        public DbSet<DepartmentChangeNotification> DepartmentChangeNotifications { get; set; }
        public DbSet<AdvanceFee> AdvanceFees { get; set; }
        public DbSet<PublishedDebtFile> PublishedDebtFiles { get; set; }
        public DbSet<UserChangesLog> UserChangesLog { get; set; }
        public DbSet<USD> USD { get; set; }
        public DbSet<EUR> EUR { get; set; }
        public DbSet<USDUYU> USDUYU { get; set; }
        public DbSet<ARSUYU> ARSUYU { get; set; }
        public DbSet<EURUSD> EURUSD { get; set; }
        public DbSet<PaymentReport> PaymentReports { get; set; }
        public DbSet<CvuEntity> CvuEntities { get; set; }
        public DbSet<CvuOperation> CvuOperations { get; set; }
        public DbSet<Echeq> Echeq { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<Cash> Cash { get; set; }
        public DbSet<Cheque> Cheque { get; set; }
        public DbSet<PaymentDetail> PaymentDetail { get; set; }
        public DbSet<PublishedDebtBankFile> PublishedDebtBankFile { get; set; }
        public DbSet<HistoricQuotations> HistoricQuotations { get; set; }
        public DbSet<PublishClient> PublishClient { get; set; }
        public DbSet<ValidacionCliente> ValidacionCliente { get; set; }
        public DbSet<DebtFree> DebtFree { get; set; }

        public const string DELETED_PROPERTY = "IsDeleted";
        public const string CREATEDON_PROPERTY = "CreatedOn";
        public const string CREATEDBY_PROPERTY = "CreatedBy";
        public const string MODIFIEDON_PROPERTY = "LastModifiedOn";
        public const string MODIFIEDBY_PROPERTY = "LastModifiedBy";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Abstract class: https://www.learnentityframeworkcore.com/inheritance/table-per-hierarchy#abstract-base-class
            modelBuilder.Entity<PaymentMethod>();
            modelBuilder.Entity<NotificationType>();
            modelBuilder.Entity<DeliveryType>();
            modelBuilder.Entity<Quotation>();
            modelBuilder.Entity<PublishedDebtFile>();
            modelBuilder.Entity<NotificationTypeRole>()
                .HasKey(ntr => new { ntr.NotificationTypeId, ntr.RoleId });
            modelBuilder.Entity<Communication>()
                .HasOne(x => x.ContactDetail)
                .WithMany(x => x.Communications)
                .HasForeignKey(x => x.ContactDetailId)
                .OnDelete(DeleteBehavior.SetNull);
            #region Audit and SoftDelete properties
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes().ToList())
            {
                Type oEntityType = Type.GetType(entityType.Name);

                // Checks if this Entity or some related entity (inheritance) is marked as SoftDelete
                if (oEntityType.HasAttribute<SoftDeleteAttribute>())
                {
                    GetType()
                       .GetMethod(nameof(ConfigureSoftDelete), BindingFlags.NonPublic | BindingFlags.Static) // Find ConfigureSoftDelete method
                       .MakeGenericMethod(oEntityType) // Set generic <T> type
                       .Invoke(null, new object[] { modelBuilder, entityType }); // Method parameters
                }

                // Checks if this Entity or some related entity (inheritance) is marked as Auditable
                if (oEntityType.HasAttribute<AuditableAttribute>())
                {
                    // Adds shadows properties

                    modelBuilder.Entity(entityType.Name).Property<DateTime?>(CREATEDON_PROPERTY).IsRequired(false);
                    modelBuilder.Entity(entityType.Name).Property<string>(CREATEDBY_PROPERTY).IsRequired(false);

                    modelBuilder.Entity(entityType.Name).Property<DateTime?>(MODIFIEDON_PROPERTY).IsRequired(false);
                    modelBuilder.Entity(entityType.Name).Property<string>(MODIFIEDBY_PROPERTY).IsRequired(false);
                }
            }

            #endregion

            #region Indexes
            // Indexes for PKs are automatically created by DB engine
            // Indexes for FKs are automatically created by EF Core


            modelBuilder.Entity<PaymentMethod>()
              .HasIndex(x => x.Payer);
            modelBuilder.Entity<PaymentMethod>()
             .HasIndex(x => x.TransactionDate);

            modelBuilder.Entity<Debin>()
            .HasIndex(x => x.IssueDate);
            modelBuilder.Entity<Debin>()
            .HasIndex(x => x.Status);

            modelBuilder.Entity<AutomaticPayment>()
            .HasIndex(x => x.Payer);
            modelBuilder.Entity<Communication>()
            .HasIndex(x => x.Client);

            modelBuilder.Entity<AutomaticPayment>()
            .HasIndex(x => x.Product);

            // You cannot create an index over a custom class, you have to use the reference id 
            // but it is shadow, so you need to do it as String
            modelBuilder.Entity<Debin>()
              .HasIndex("BankAccountId");

            modelBuilder.Entity<BankAccount>()
              .HasIndex(x => x.ClientCuit);
            modelBuilder.Entity<BankAccount>()
            .HasIndex(x => x.Cbu);
            modelBuilder.Entity<BankAccount>()
            .HasIndex(x => x.Status);
            modelBuilder.Entity<BankAccount>()
            .HasIndex(x => new { x.ClientCuit, x.Cbu, x.Currency })
            //This make index filtered
            .HasFilter("IsDeleted = 0")
            .IsUnique();

            modelBuilder.Entity<ExchangeRateFile>()
                .HasIndex(x => x.FileName)
                .IsUnique();
            modelBuilder.Entity<ArchivoDeuda>()
            .HasIndex(x => x.FormatedFileName);
            modelBuilder.Entity<ArchivoDeuda>()
            .HasIndex(x => x.FileName)
            .IsUnique();

            modelBuilder.Entity<DetalleDeuda>()
            .HasKey(x => x.Id)
            .IsClustered(false);
            modelBuilder.Entity<DetalleDeuda>()
            .HasIndex(x => x.NroCuitCliente);
            modelBuilder.Entity<DetalleDeuda>()
           .HasIndex(x => x.NroComprobante);
            modelBuilder.Entity<DetalleDeuda>()
            .HasIndex(x => x.FechaPrimerVenc);
            modelBuilder.Entity<DetalleDeuda>()
            .HasIndex(x => x.ArchivoDeudaId)
            .IsClustered();

            // No incluye NroCuitCliente porque por ahora no quierne que haya publicacion de la misma cuota a personas distintas.
            // Al estar el cuit en el unique, no romperia, al sacarlo si porque los campos estos si son unicos.
            modelBuilder.Entity<DetalleDeuda>()
            .HasIndex(x => new { x.ArchivoDeudaId, x.NroComprobante, x.FechaPrimerVenc, x.CodigoMoneda, x.ObsLibreSegunda /*,x.NroCuitCliente*/ }).IsUnique();

            #endregion

            #region Identity autoincremental

            modelBuilder.Entity<PaymentMethod>()
              .Property(x => x.Id)
              .ValueGeneratedOnAdd();

            modelBuilder.Entity<AutomaticPayment>()
             .Property(x => x.Id)
             .ValueGeneratedOnAdd();

            modelBuilder.Entity<AnonymousPayment>()
             .Property(x => x.Id)
             .ValueGeneratedOnAdd();

            modelBuilder.Entity<BankAccount>()
             .Property(x => x.Id)
             .ValueGeneratedOnAdd();

            modelBuilder.Entity<AccountBalance>()
             .Property(x => x.Id)
             .ValueGeneratedOnAdd();

            modelBuilder.Entity<ExchangeRateFile>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<NotificationUser>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Notification>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Template>()
               .Property(x => x.Id)
               .ValueGeneratedOnAdd();

            modelBuilder.Entity<NotificationType>()
               .Property(x => x.Id)
               .ValueGeneratedOnAdd();

            modelBuilder.Entity<NotificationTypeRole>()
               .Property(x => x.Id)
               .ValueGeneratedOnAdd();

            modelBuilder.Entity<DeliveryType>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ArchivoDeuda>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<HeaderDeuda>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<DetalleDeuda>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TrailerDeuda>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OrganismoDeuda>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Template>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TemplateTokenReference>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ContactDetail>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Communication>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Quotation>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<PublishDebtRejectionFile>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<PublishDebtRejection>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<PublishDebtRejectionError>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<AdvanceFee>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<PublishedDebtFile>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<PaymentReport>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CvuEntity>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            #endregion

            #region Conversions

            ValueConverter<User, string> clientConverter = new ValueConverter<User, string>(
                client => client.Id,
                clientId => new User { Id = clientId }
            );

            modelBuilder.Entity<Permission>()
                .Property(x => x.Code)
                .HasConversion<int>();

            modelBuilder.Entity<PaymentMethod>()
                .Property(x => x.Payer).HasConversion(clientConverter);

            modelBuilder.Entity<Debin>()
                .Property(x => x.Payer).HasConversion(clientConverter);

            modelBuilder.Entity<AutomaticPayment>()
                .Property(x => x.Payer).HasConversion(clientConverter);

            modelBuilder.Entity<NotificationUser>()
                .Property(x => x.User).HasConversion(clientConverter);

            modelBuilder.Entity<Communication>()
                .Property(x => x.Client).HasConversion(clientConverter);

            #endregion

            #region Ignored Tables
            modelBuilder.Ignore<BaseEntity>();

            #endregion

        }

        private static void ConfigureSoftDelete<T>(ModelBuilder modelBuilder, IMutableEntityType entityType) where T : class
        {
            // Gets the PK name for this entity
            string PK_Name = entityType.FindPrimaryKey().Properties.Select(x => x.Name).Single();

            // Adds shadow property
            modelBuilder.Entity<T>().Property<bool>(DELETED_PROPERTY).IsRequired();

            modelBuilder.Entity<T>().HasIndex(PK_Name, DELETED_PROPERTY);

            // QueryFilter can't be applied over child entities in inheritance.

            IEnumerable<Type> inheritanceHierarchy = typeof(T).GetInheritanceHierarchy();
            Type LastFatherType = inheritanceHierarchy.Last(x => x != typeof(object));

            typeof(RelationalDbContext).GetMethod(nameof(SoftDeleteQueryFilter), BindingFlags.NonPublic | BindingFlags.Static) // Find SoftDeleteQueryFilter method
            .MakeGenericMethod(LastFatherType) // Set generic <T> type
            .Invoke(null, new object[] { modelBuilder }); // Method parameters


        }
        /// <summary>
        /// Method used to be called with generics
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelBuilder"></param>
        private static void SoftDeleteQueryFilter<T>(ModelBuilder modelBuilder) where T : class
        {
            modelBuilder.Entity<T>().HasQueryFilter(x =>
           EF.Property<bool>(x, DELETED_PROPERTY) == false);
        }

        #region SoftDelete SaveChanges Override
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        public override int SaveChanges()
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChanges();
        }


        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateSoftDeleteStatuses()
        {
            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries())
            {
                if (entry.Entity.GetType().HasAttribute<SoftDeleteAttribute>())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.CurrentValues[DELETED_PROPERTY] = false;
                            break;
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            entry.CurrentValues[DELETED_PROPERTY] = true;
                            break;
                    }
                }
            }
        }

        private IDbContextTransaction _currentTransaction;
        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;
        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null) return null!;

            _currentTransaction = await Database.BeginTransactionAsync();

            return _currentTransaction;
        }
        public IDbContextTransaction BeginTransaction()
        {
            if (_currentTransaction != null) return null!;

            _currentTransaction = Database.BeginTransaction();

            return _currentTransaction;
        }

        public Task RunWithExecutionStrategyAsync(Func<Task> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            var strategy = Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(
                async () =>
                {
                    using (var transaction = await BeginTransactionAsync())
                    {
                        await action();
                        await CommitAsync(transaction);
                        return Task.CompletedTask;
                    }
                });
        }

        public void RunWithExecutionStrategy(Action action, CancellationToken cancellationToken = default(CancellationToken))
        {
            var strategy = Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using (var transaction = BeginTransaction())
                {
                    action();
                    Commit(transaction);
                }
            });
        }

        public async Task CommitAsync(IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

            try
            {
                await SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null!;
                }
            }
        }
        public void Commit(IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

            try
            {
                SaveChanges();
                transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null!;
                }
            }
        }

        private void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null!;
                }
            }
        }

        #endregion
    }
}


