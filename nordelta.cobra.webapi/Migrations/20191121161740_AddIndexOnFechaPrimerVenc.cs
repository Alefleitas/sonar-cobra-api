using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddIndexOnFechaPrimerVenc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_FechaPrimerVenc",
                table: "DetallesDeuda",
                column: "FechaPrimerVenc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DetallesDeuda_FechaPrimerVenc",
                table: "DetallesDeuda");
        }
    }
}
