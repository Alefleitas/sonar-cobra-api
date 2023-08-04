using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddLockboxNameAndInformedDateColumnsToPaymentMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InformedDate",
                table: "PaymentMethod",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockboxName",
                table: "PaymentMethod",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InformedDate",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "LockboxName",
                table: "PaymentMethod");
        }
    }
}
