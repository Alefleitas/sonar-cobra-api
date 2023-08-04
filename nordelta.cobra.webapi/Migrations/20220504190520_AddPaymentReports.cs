using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddPaymentReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentReportId",
                table: "DetallesDeuda",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayerId = table.Column<string>(nullable: true),
                    ReportDate = table.Column<DateTime>(nullable: false),
                    Cuit = table.Column<string>(nullable: true),
                    Currency = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Product = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_PaymentReportId",
                table: "DetallesDeuda",
                column: "PaymentReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesDeuda_PaymentReports_PaymentReportId",
                table: "DetallesDeuda",
                column: "PaymentReportId",
                principalTable: "PaymentReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesDeuda_PaymentReports_PaymentReportId",
                table: "DetallesDeuda");

            migrationBuilder.DropTable(
                name: "PaymentReports");

            migrationBuilder.DropIndex(
                name: "IX_DetallesDeuda_PaymentReportId",
                table: "DetallesDeuda");

            migrationBuilder.DropColumn(
                name: "PaymentReportId",
                table: "DetallesDeuda");
        }
    }
}
