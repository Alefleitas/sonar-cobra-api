using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddPublishDebtToAccountBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedPublishDebtBy",
                table: "AccountBalances",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishDebt",
                table: "AccountBalances",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedPublishDebtBy",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "PublishDebt",
                table: "AccountBalances");
        }
    }
}
