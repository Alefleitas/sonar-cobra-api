using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class RefactorPaymentMethodAndRefactorPaymentReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "PaymentReports");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportDateVto",
                table: "PaymentReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OlapMethod",
                table: "PaymentMethod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportDateVto",
                table: "PaymentReports");

            migrationBuilder.DropColumn(
                name: "OlapMethod",
                table: "PaymentMethod");

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "PaymentReports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
