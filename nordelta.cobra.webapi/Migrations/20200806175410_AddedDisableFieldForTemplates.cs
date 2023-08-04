using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddedDisableFieldForTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Template",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Template");
        }
    }
}
