using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class updateIndexBankAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_ClientCuit_Cbu_Currency",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_ClientCuit_Cbu_Currency",
                table: "BankAccounts",
                columns: new[] { "ClientCuit", "Cbu", "Currency" },
                unique: true,
                filter: "IsDeleted = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_ClientCuit_Cbu_Currency",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_ClientCuit_Cbu_Currency",
                table: "BankAccounts",
                columns: new[] { "ClientCuit", "Cbu", "Currency" },
                unique: true);
        }
    }
}
