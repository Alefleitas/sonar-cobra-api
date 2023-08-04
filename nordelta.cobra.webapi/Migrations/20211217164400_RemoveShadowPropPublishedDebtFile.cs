using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class RemoveShadowPropPublishedDebtFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PublishedDebtFiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "PublishedDebtFiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "PublishedDebtFiles");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "PublishedDebtFiles",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "PublishedDebtFiles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PublishedDebtFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "PublishedDebtFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "PublishedDebtFiles",
                type: "datetime2",
                nullable: true);
        }
    }
}
