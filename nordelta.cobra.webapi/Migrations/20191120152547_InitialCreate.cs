using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountBalances",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Cbu = table.Column<string>(nullable: false),
                    Cuit = table.Column<string>(nullable: false),
                    ClientCuit = table.Column<string>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Currency = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SocialReason = table.Column<string>(nullable: true),
                    Cuit = table.Column<string>(nullable: true),
                    CbuPeso = table.Column<string>(nullable: true),
                    CbuDolar = table.Column<string>(nullable: true),
                    ConvenioPeso = table.Column<string>(nullable: true),
                    ConvenioDolar = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganismosDeuda",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CuitEmpresa = table.Column<string>(nullable: true),
                    NroDigitoEmpresa = table.Column<string>(nullable: true),
                    CodProducto = table.Column<string>(nullable: true),
                    NroAcuerdo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganismosDeuda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "TrailersDeuda",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TipoRegistro = table.Column<string>(nullable: true),
                    ImporteTotalPrimerVencimiento = table.Column<string>(nullable: true),
                    Ceros = table.Column<string>(nullable: true),
                    CantRegistroInformados = table.Column<string>(nullable: true),
                    Relleno = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrailersDeuda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Payer = table.Column<string>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    Currency = table.Column<int>(nullable: false),
                    TransactionDate = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true),
                    DebinCode = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    Type = table.Column<int>(nullable: true),
                    IssueDate = table.Column<DateTime>(nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    BankAccountId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethod_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeadersDeuda",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TipoRegistro = table.Column<string>(nullable: true),
                    CodOrganismo = table.Column<string>(nullable: true),
                    CodCanal = table.Column<string>(nullable: true),
                    NroEnvio = table.Column<string>(nullable: true),
                    UltimaRendicionProcesada = table.Column<string>(nullable: true),
                    MarcaActualizacionCuentaComercial = table.Column<string>(nullable: true),
                    MarcaPublicacionOnline = table.Column<string>(nullable: true),
                    Relleno = table.Column<string>(nullable: true),
                    OrganismoId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeadersDeuda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeadersDeuda_OrganismosDeuda_OrganismoId",
                        column: x => x.OrganismoId,
                        principalTable: "OrganismosDeuda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PermissionsXRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true),
                    RoleName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsXRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionsXRoles_Roles_RoleName",
                        column: x => x.RoleName,
                        principalTable: "Roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosDeuda",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TimeStamp = table.Column<string>(nullable: false),
                    FormatedFileName = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    HeaderId = table.Column<int>(nullable: true),
                    TrailerId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosDeuda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosDeuda_HeadersDeuda_HeaderId",
                        column: x => x.HeaderId,
                        principalTable: "HeadersDeuda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchivosDeuda_TrailersDeuda_TrailerId",
                        column: x => x.TrailerId,
                        principalTable: "TrailersDeuda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesDeuda",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TipoRegistro = table.Column<string>(nullable: true),
                    TipoOperacion = table.Column<string>(nullable: true),
                    CodigoMoneda = table.Column<string>(maxLength: 200, nullable: true),
                    NumeroCliente = table.Column<string>(nullable: true),
                    TipoComprobante = table.Column<string>(nullable: true),
                    NroComprobante = table.Column<string>(maxLength: 200, nullable: true),
                    NroCuota = table.Column<string>(nullable: true),
                    NombreCliente = table.Column<string>(nullable: true),
                    DireccionCliente = table.Column<string>(nullable: true),
                    DescripcionLocalidad = table.Column<string>(nullable: true),
                    PrefijoCodPostal = table.Column<string>(nullable: true),
                    NroCodPostal = table.Column<string>(nullable: true),
                    UbicManzanaCodPostal = table.Column<string>(nullable: true),
                    FechaPrimerVenc = table.Column<string>(maxLength: 200, nullable: true),
                    ImportePrimerVenc = table.Column<string>(nullable: true),
                    FechaSegundoVenc = table.Column<string>(nullable: true),
                    ImporteSegundoVenc = table.Column<string>(nullable: true),
                    FechaHastaDescuento = table.Column<string>(nullable: true),
                    ImporteProntoPago = table.Column<string>(nullable: true),
                    FechaHastaPunitorios = table.Column<string>(nullable: true),
                    TasaPunitorios = table.Column<string>(nullable: true),
                    MarcaExcepcionCobroComisionDepositante = table.Column<string>(nullable: true),
                    FormasCobroPermitidas = table.Column<string>(nullable: true),
                    NroCuitCliente = table.Column<string>(maxLength: 200, nullable: true),
                    CodIngresosBrutos = table.Column<string>(nullable: true),
                    CodCondicionIva = table.Column<string>(nullable: true),
                    CodConcepto = table.Column<string>(nullable: true),
                    DescCodigo = table.Column<string>(nullable: true),
                    ObsLibrePrimera = table.Column<string>(nullable: true),
                    ObsLibreSegunda = table.Column<string>(maxLength: 200, nullable: true),
                    ObsLibreTercera = table.Column<string>(nullable: true),
                    ObsLibreCuarta = table.Column<string>(nullable: true),
                    Relleno = table.Column<string>(nullable: true),
                    ArchivoDeudaId = table.Column<int>(nullable: false),
                    DebinId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesDeuda", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_DetallesDeuda_ArchivosDeuda_ArchivoDeudaId",
                        column: x => x.ArchivoDeudaId,
                        principalTable: "ArchivosDeuda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesDeuda_PaymentMethod_DebinId",
                        column: x => x.DebinId,
                        principalTable: "PaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_Id_IsDeleted",
                table: "AccountBalances",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosDeuda_HeaderId",
                table: "ArchivosDeuda",
                column: "HeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosDeuda_TrailerId",
                table: "ArchivosDeuda",
                column: "TrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Cbu",
                table: "BankAccounts",
                column: "Cbu");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_ClientCuit",
                table: "BankAccounts",
                column: "ClientCuit");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Status",
                table: "BankAccounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Id_IsDeleted",
                table: "BankAccounts",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_ClientCuit_Cbu_Currency",
                table: "BankAccounts",
                columns: new[] { "ClientCuit", "Cbu", "Currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_ArchivoDeudaId",
                table: "DetallesDeuda",
                column: "ArchivoDeudaId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_DebinId",
                table: "DetallesDeuda",
                column: "DebinId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_NroComprobante",
                table: "DetallesDeuda",
                column: "NroComprobante");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_NroCuitCliente",
                table: "DetallesDeuda",
                column: "NroCuitCliente");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_ArchivoDeudaId_NroComprobante_FechaPrimerVenc_CodigoMoneda_ObsLibreSegunda_NroCuitCliente",
                table: "DetallesDeuda",
                columns: new[] { "ArchivoDeudaId", "NroComprobante", "FechaPrimerVenc", "CodigoMoneda", "ObsLibreSegunda", "NroCuitCliente" },
                unique: true,
                filter: "[NroComprobante] IS NOT NULL AND [FechaPrimerVenc] IS NOT NULL AND [CodigoMoneda] IS NOT NULL AND [ObsLibreSegunda] IS NOT NULL AND [NroCuitCliente] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HeadersDeuda_OrganismoId",
                table: "HeadersDeuda",
                column: "OrganismoId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_BankAccountId",
                table: "PaymentMethod",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_IssueDate",
                table: "PaymentMethod",
                column: "IssueDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_Status",
                table: "PaymentMethod",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_Payer",
                table: "PaymentMethod",
                column: "Payer");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_TransactionDate",
                table: "PaymentMethod",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_Id_IsDeleted",
                table: "PaymentMethod",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionsXRoles_RoleName",
                table: "PermissionsXRoles",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name_IsDeleted",
                table: "Roles",
                columns: new[] { "Name", "IsDeleted" });

            migrationBuilder.CreateAuditTriggersForAuditableEntities(TargetModel);
            migrationBuilder.CreateAuditTable();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBalances");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "DetallesDeuda");

            migrationBuilder.DropTable(
                name: "PermissionsXRoles");

            migrationBuilder.DropTable(
                name: "ArchivosDeuda");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "HeadersDeuda");

            migrationBuilder.DropTable(
                name: "TrailersDeuda");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "OrganismosDeuda");
        }
    }
}
