using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddEcheqAndStatusColumnInPaymentReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PaymentReports",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EcheqCreditingDate",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EcheqOperationId",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EcheqStatus",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EcheqType",
                table: "PaymentMethod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PaymentReports");

            migrationBuilder.DropColumn(
                name: "EcheqCreditingDate",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "EcheqOperationId",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "EcheqStatus",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "EcheqType",
                table: "PaymentMethod");
        }
    }
}
