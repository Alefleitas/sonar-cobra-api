using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddStatusToAdvanceFee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Informed",
                table: "AdvanceFees");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AdvanceFees",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AdvanceFees");

            migrationBuilder.AddColumn<bool>(
                name: "Informed",
                table: "AdvanceFees",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
