using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class FixedContactDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_NotificationTypeXRole_Id",
                table: "NotificationTypeXRole");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_NotificationTypeXRole_Id",
                table: "NotificationTypeXRole",
                column: "Id");
        }
    }
}
