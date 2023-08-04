using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddValidacionClienteModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValidacionCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistryId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyCreationDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrgOrigSystemReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerFlag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierFlag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VendorNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JgzzFiscalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxRegimeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultRegistrationFlag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccOrigSystemReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CasOrigSystemReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartySiteOsdr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationOsdr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocAttribute1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteUseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefAcctBu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidacionCliente", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValidacionCliente");
        }
    }
}
