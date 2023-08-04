using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddPublishedDebtBankFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublishedDebtBankFile",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArchivoDeudaId = table.Column<int>(nullable: false),
                    CuitEmpresa = table.Column<string>(nullable: true),
                    CodigoOrganismo = table.Column<string>(nullable: true),
                    NroArchivo = table.Column<int>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    Bank = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedDebtBankFile", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublishedDebtBankFile");
        }
    }
}
