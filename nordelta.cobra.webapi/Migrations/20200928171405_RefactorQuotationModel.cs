using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class RefactorQuotationModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Especie",
                table: "Quotations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromCurrency",
                table: "Quotations",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RateType",
                table: "Quotations",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToCurrency",
                table: "Quotations",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Valor",
                table: "Quotations",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especie",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "FromCurrency",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RateType",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "ToCurrency",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Quotations");
        }
    }
}
