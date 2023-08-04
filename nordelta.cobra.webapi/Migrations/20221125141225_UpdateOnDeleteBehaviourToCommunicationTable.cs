using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class UpdateOnDeleteBehaviourToCommunicationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communications_ContactDetails_ContactDetailId",
                table: "Communications");

            migrationBuilder.AddForeignKey(
                name: "FK_Communications_ContactDetails_ContactDetailId",
                table: "Communications",
                column: "ContactDetailId",
                principalTable: "ContactDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communications_ContactDetails_ContactDetailId",
                table: "Communications");

            migrationBuilder.AddForeignKey(
                name: "FK_Communications_ContactDetails_ContactDetailId",
                table: "Communications",
                column: "ContactDetailId",
                principalTable: "ContactDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
