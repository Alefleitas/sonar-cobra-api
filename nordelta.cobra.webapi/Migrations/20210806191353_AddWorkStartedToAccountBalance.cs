using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddWorkStartedToAccountBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkStarted",
                table: "AccountBalances",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkStarted",
                table: "AccountBalances");
        }
    }
}
