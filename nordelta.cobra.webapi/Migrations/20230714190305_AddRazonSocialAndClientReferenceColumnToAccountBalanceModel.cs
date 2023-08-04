using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddRazonSocialAndClientReferenceColumnToAccountBalanceModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientReference",
                table: "AccountBalances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "AccountBalances",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientReference",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "AccountBalances");
        }
    }
}
