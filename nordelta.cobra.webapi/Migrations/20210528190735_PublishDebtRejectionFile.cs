using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class PublishDebtRejectionFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchivoDeudaRechazo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(nullable: true),
                    FileDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivoDeudaRechazo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetalleDeudaRechazo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublishDebtRejectionFileId = table.Column<int>(nullable: false),
                    Empresa = table.Column<string>(nullable: true),
                    UltimaRendicionProcesada = table.Column<int>(nullable: false),
                    NroCliente = table.Column<string>(nullable: true),
                    CuitCliente = table.Column<string>(nullable: true),
                    Moneda = table.Column<string>(nullable: true),
                    NroCuota = table.Column<int>(nullable: false),
                    TipoComprobante = table.Column<string>(nullable: true),
                    NroComprobante = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleDeudaRechazo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleDeudaRechazo_ArchivoDeudaRechazo_PublishDebtRejectionFileId",
                        column: x => x.PublishDebtRejectionFileId,
                        principalTable: "ArchivoDeudaRechazo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleDeudaRechazoError",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoError = table.Column<int>(nullable: false),
                    DescripcionError = table.Column<string>(nullable: true),
                    PublishDebtRejectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleDeudaRechazoError", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleDeudaRechazoError_DetalleDeudaRechazo_PublishDebtRejectionId",
                        column: x => x.PublishDebtRejectionId,
                        principalTable: "DetalleDeudaRechazo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleDeudaRechazo_PublishDebtRejectionFileId",
                table: "DetalleDeudaRechazo",
                column: "PublishDebtRejectionFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleDeudaRechazoError_PublishDebtRejectionId",
                table: "DetalleDeudaRechazoError",
                column: "PublishDebtRejectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleDeudaRechazoError");

            migrationBuilder.DropTable(
                name: "DetalleDeudaRechazo");

            migrationBuilder.DropTable(
                name: "ArchivoDeudaRechazo");
        }
    }
}
