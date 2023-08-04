using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nordelta.cobra.webapi.Migrations
{
    public partial class ChangeDebinExternalCodeType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExternalCode",
                table: "PaymentMethod",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ExternalCode",
                table: "PaymentMethod",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
