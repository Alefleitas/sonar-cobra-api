using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddedBUToAccountBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessUnit",
                table: "AccountBalances",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessUnit",
                table: "AccountBalances");
        }
    }
}
