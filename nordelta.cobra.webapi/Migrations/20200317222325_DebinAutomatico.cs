using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class DebinAutomatico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ExchangeRateFile",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AutomaticPayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Payer = table.Column<string>(nullable: false),
                    Currency = table.Column<int>(nullable: false),
                    BankAccountId = table.Column<int>(nullable: false),
                    Product = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomaticPayments_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateFile_FileName",
                table: "ExchangeRateFile",
                column: "FileName",
                unique: true,
                filter: "[FileName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticPayments_BankAccountId",
                table: "AutomaticPayments",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticPayments_Payer",
                table: "AutomaticPayments",
                column: "Payer");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticPayments_Product",
                table: "AutomaticPayments",
                column: "Product",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticPayments_Id_IsDeleted",
                table: "AutomaticPayments",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateAuditTriggersForAuditableEntities(TargetModel);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomaticPayments");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRateFile_FileName",
                table: "ExchangeRateFile");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ExchangeRateFile",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
