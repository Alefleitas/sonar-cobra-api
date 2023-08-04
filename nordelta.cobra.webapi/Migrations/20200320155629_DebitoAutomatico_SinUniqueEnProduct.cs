using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class DebitoAutomatico_SinUniqueEnProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AutomaticPayments_Product",
                table: "AutomaticPayments");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticPayments_Product",
                table: "AutomaticPayments",
                column: "Product");

            migrationBuilder.CreateAuditTriggersForAuditableEntities(TargetModel);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AutomaticPayments_Product",
                table: "AutomaticPayments");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticPayments_Product",
                table: "AutomaticPayments",
                column: "Product",
                unique: true);
        }
    }
}
