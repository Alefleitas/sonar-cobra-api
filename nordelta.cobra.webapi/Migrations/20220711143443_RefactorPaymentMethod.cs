using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class RefactorPaymentMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "PaymentReports");

            migrationBuilder.DropColumn(
                name: "CvuOperationStatus",
                table: "PaymentMethod");

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

            migrationBuilder.AddColumn<int>(
                name: "Instrument",
                table: "PaymentReports",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "PaymentReports",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PaymentMethod",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoOrganismo",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreditingDate",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Instrument",
                table: "PaymentMethod",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "PaymentMethod",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instrument",
                table: "PaymentReports");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "PaymentReports");

            migrationBuilder.DropColumn(
                name: "CodigoOrganismo",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CreditingDate",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "Instrument",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "PaymentMethod");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "PaymentReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PaymentMethod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "CvuOperationStatus",
                table: "PaymentMethod",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EcheqCreditingDate",
                table: "PaymentMethod",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EcheqOperationId",
                table: "PaymentMethod",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EcheqStatus",
                table: "PaymentMethod",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EcheqType",
                table: "PaymentMethod",
                type: "int",
                nullable: true);
        }
    }
}
