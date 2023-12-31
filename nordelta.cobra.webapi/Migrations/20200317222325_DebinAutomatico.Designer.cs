﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using nordelta.cobra.webapi.Repositories.Contexts;

namespace nordelta.cobra.webapi.Migrations
{
    [DbContext(typeof(RelationalDbContext))]
    [Migration("20200317222325_DebinAutomatico")]
    partial class DebinAutomatico
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("nordelta.cobra.webapi.Models.AccountBalance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedOn");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("LastModifiedBy");

                    b.Property<DateTime?>("LastModifiedOn");

                    b.HasKey("Id");

                    b.HasIndex("Id", "IsDeleted");

                    b.ToTable("AccountBalances");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.AutomaticPayment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BankAccountId");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedOn");

                    b.Property<int>("Currency");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("LastModifiedBy");

                    b.Property<DateTime?>("LastModifiedOn");

                    b.Property<string>("Payer")
                        .IsRequired();

                    b.Property<string>("Product")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("BankAccountId");

                    b.HasIndex("Payer");

                    b.HasIndex("Product")
                        .IsUnique();

                    b.HasIndex("Id", "IsDeleted");

                    b.ToTable("AutomaticPayments");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.BankAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Cbu")
                        .IsRequired();

                    b.Property<string>("ClientCuit")
                        .IsRequired();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedOn");

                    b.Property<string>("Cuit")
                        .IsRequired();

                    b.Property<int>("Currency");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("LastModifiedBy");

                    b.Property<DateTime?>("LastModifiedOn");

                    b.Property<int>("Status");

                    b.HasKey("Id");

                    b.HasIndex("Cbu");

                    b.HasIndex("ClientCuit");

                    b.HasIndex("Status");

                    b.HasIndex("Id", "IsDeleted");

                    b.HasIndex("ClientCuit", "Cbu", "Currency")
                        .IsUnique();

                    b.ToTable("BankAccounts");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CbuDolar");

                    b.Property<string>("CbuPeso");

                    b.Property<string>("ConvenioDolar");

                    b.Property<string>("ConvenioPeso");

                    b.Property<string>("Cuit");

                    b.Property<string>("SocialReason");

                    b.HasKey("Id");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.ExchangeRateFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("FileName");

                    b.Property<string>("TimeStamp");

                    b.Property<string>("UvaExchangeRate");

                    b.HasKey("Id");

                    b.HasIndex("FileName")
                        .IsUnique()
                        .HasFilter("[FileName] IS NOT NULL");

                    b.ToTable("ExchangeRateFile");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.PaymentMethod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("Amount");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedOn");

                    b.Property<int>("Currency");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("LastModifiedBy");

                    b.Property<DateTime?>("LastModifiedOn");

                    b.Property<string>("Payer")
                        .IsRequired();

                    b.Property<DateTime>("TransactionDate");

                    b.HasKey("Id");

                    b.HasIndex("Payer");

                    b.HasIndex("TransactionDate");

                    b.HasIndex("Id", "IsDeleted");

                    b.ToTable("PaymentMethod");

                    b.HasDiscriminator<string>("Discriminator").HasValue("PaymentMethod");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Code");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedOn");

                    b.Property<string>("LastModifiedBy");

                    b.Property<DateTime?>("LastModifiedOn");

                    b.Property<string>("RoleName");

                    b.HasKey("Id");

                    b.HasIndex("RoleName");

                    b.ToTable("PermissionsXRoles");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.Role", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedOn");

                    b.Property<string>("Description");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("LastModifiedBy");

                    b.Property<DateTime?>("LastModifiedOn");

                    b.HasKey("Name");

                    b.HasIndex("Name", "IsDeleted");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.ArchivoDeuda", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("FileName");

                    b.Property<string>("FormatedFileName");

                    b.Property<int?>("HeaderId");

                    b.Property<string>("TimeStamp")
                        .IsRequired();

                    b.Property<int?>("TrailerId");

                    b.HasKey("Id");

                    b.HasIndex("FileName")
                        .IsUnique()
                        .HasFilter("[FileName] IS NOT NULL");

                    b.HasIndex("FormatedFileName");

                    b.HasIndex("HeaderId");

                    b.HasIndex("TrailerId");

                    b.ToTable("ArchivosDeuda");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.DetalleDeuda", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ArchivoDeudaId");

                    b.Property<string>("CodConcepto");

                    b.Property<string>("CodCondicionIva");

                    b.Property<string>("CodIngresosBrutos");

                    b.Property<string>("CodigoMoneda")
                        .HasMaxLength(200);

                    b.Property<int?>("DebinId");

                    b.Property<string>("DescCodigo");

                    b.Property<string>("DescripcionLocalidad");

                    b.Property<string>("DireccionCliente");

                    b.Property<string>("FechaHastaDescuento");

                    b.Property<string>("FechaHastaPunitorios");

                    b.Property<string>("FechaPrimerVenc")
                        .HasMaxLength(200);

                    b.Property<string>("FechaSegundoVenc");

                    b.Property<string>("FormasCobroPermitidas");

                    b.Property<string>("ImportePrimerVenc");

                    b.Property<string>("ImporteProntoPago");

                    b.Property<string>("ImporteSegundoVenc");

                    b.Property<string>("MarcaExcepcionCobroComisionDepositante");

                    b.Property<string>("NombreCliente");

                    b.Property<string>("NroCodPostal");

                    b.Property<string>("NroComprobante")
                        .HasMaxLength(200);

                    b.Property<string>("NroCuitCliente")
                        .HasMaxLength(200);

                    b.Property<string>("NroCuota");

                    b.Property<string>("NumeroCliente");

                    b.Property<string>("ObsLibreCuarta");

                    b.Property<string>("ObsLibrePrimera");

                    b.Property<string>("ObsLibreSegunda")
                        .HasMaxLength(200);

                    b.Property<string>("ObsLibreTercera");

                    b.Property<string>("PrefijoCodPostal");

                    b.Property<string>("Relleno");

                    b.Property<string>("TasaPunitorios");

                    b.Property<string>("TipoComprobante");

                    b.Property<string>("TipoOperacion");

                    b.Property<string>("TipoRegistro");

                    b.Property<string>("UbicManzanaCodPostal");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("ArchivoDeudaId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("DebinId");

                    b.HasIndex("FechaPrimerVenc");

                    b.HasIndex("NroComprobante");

                    b.HasIndex("NroCuitCliente");

                    b.HasIndex("ArchivoDeudaId", "NroComprobante", "FechaPrimerVenc", "CodigoMoneda", "ObsLibreSegunda")
                        .IsUnique()
                        .HasFilter("[NroComprobante] IS NOT NULL AND [FechaPrimerVenc] IS NOT NULL AND [CodigoMoneda] IS NOT NULL AND [ObsLibreSegunda] IS NOT NULL");

                    b.ToTable("DetallesDeuda");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.HeaderDeuda", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodCanal");

                    b.Property<string>("CodOrganismo");

                    b.Property<string>("MarcaActualizacionCuentaComercial");

                    b.Property<string>("MarcaPublicacionOnline");

                    b.Property<string>("NroEnvio");

                    b.Property<int?>("OrganismoId");

                    b.Property<string>("Relleno");

                    b.Property<string>("TipoRegistro");

                    b.Property<string>("UltimaRendicionProcesada");

                    b.HasKey("Id");

                    b.HasIndex("OrganismoId");

                    b.ToTable("HeadersDeuda");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.OrganismoDeuda", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodProducto");

                    b.Property<string>("CuitEmpresa");

                    b.Property<string>("NroAcuerdo");

                    b.Property<string>("NroDigitoEmpresa");

                    b.HasKey("Id");

                    b.ToTable("OrganismosDeuda");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.TrailerDeuda", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CantRegistroInformados");

                    b.Property<string>("Ceros");

                    b.Property<string>("ImporteTotalPrimerVencimiento");

                    b.Property<string>("Relleno");

                    b.Property<string>("TipoRegistro");

                    b.HasKey("Id");

                    b.ToTable("TrailersDeuda");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.Debin", b =>
                {
                    b.HasBaseType("nordelta.cobra.webapi.Models.PaymentMethod");

                    b.Property<int>("BankAccountId");

                    b.Property<string>("DebinCode")
                        .IsRequired();

                    b.Property<DateTime>("ExpirationDate");

                    b.Property<DateTime>("IssueDate");

                    b.Property<int>("Status");

                    b.Property<int>("Type");

                    b.Property<string>("VendorCuit");

                    b.HasIndex("BankAccountId");

                    b.HasIndex("IssueDate");

                    b.HasIndex("Status");

                    b.HasDiscriminator().HasValue("Debin");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.AutomaticPayment", b =>
                {
                    b.HasOne("nordelta.cobra.webapi.Models.BankAccount", "BankAccount")
                        .WithMany()
                        .HasForeignKey("BankAccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.Permission", b =>
                {
                    b.HasOne("nordelta.cobra.webapi.Models.Role")
                        .WithMany("Permissions")
                        .HasForeignKey("RoleName");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.ArchivoDeuda", b =>
                {
                    b.HasOne("nordelta.cobra.webapi.Services.DTOs.HeaderDeuda", "Header")
                        .WithMany()
                        .HasForeignKey("HeaderId");

                    b.HasOne("nordelta.cobra.webapi.Services.DTOs.TrailerDeuda", "Trailer")
                        .WithMany()
                        .HasForeignKey("TrailerId");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.DetalleDeuda", b =>
                {
                    b.HasOne("nordelta.cobra.webapi.Services.DTOs.ArchivoDeuda", "ArchivoDeuda")
                        .WithMany()
                        .HasForeignKey("ArchivoDeudaId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("nordelta.cobra.webapi.Models.Debin", "Debin")
                        .WithMany("Debts")
                        .HasForeignKey("DebinId");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Services.DTOs.HeaderDeuda", b =>
                {
                    b.HasOne("nordelta.cobra.webapi.Services.DTOs.OrganismoDeuda", "Organismo")
                        .WithMany()
                        .HasForeignKey("OrganismoId");
                });

            modelBuilder.Entity("nordelta.cobra.webapi.Models.Debin", b =>
                {
                    b.HasOne("nordelta.cobra.webapi.Models.BankAccount", "BankAccount")
                        .WithMany()
                        .HasForeignKey("BankAccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
