using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddedEffectiveDatesAndUserForQuotatinos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Quotations");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDateFrom",
                table: "Quotations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDateTo",
                table: "Quotations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadDate",
                table: "Quotations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Quotations",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDateFrom",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "EffectiveDateTo",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "UploadDate",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Quotations");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Quotations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
