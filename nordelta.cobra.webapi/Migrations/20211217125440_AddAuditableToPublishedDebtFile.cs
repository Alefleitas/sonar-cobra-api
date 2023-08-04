using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddAuditableToPublishedDebtFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PublishedDebtFiles",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "PublishedDebtFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "PublishedDebtFiles",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "PublishedDebtFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PublishedDebtFiles");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "PublishedDebtFiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "PublishedDebtFiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "PublishedDebtFiles");
        }
    }
}
