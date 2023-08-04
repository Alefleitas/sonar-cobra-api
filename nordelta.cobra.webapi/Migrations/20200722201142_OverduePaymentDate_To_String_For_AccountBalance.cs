using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class OverduePaymentDate_To_String_For_AccountBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OverduePaymentDate",
                table: "AccountBalances",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "OverduePaymentDate",
                table: "AccountBalances",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
