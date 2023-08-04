using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddRepeatedDebtDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepeatedDebtDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(nullable: false),
                    NroComprobante = table.Column<string>(nullable: false),
                    FechaPrimerVenc = table.Column<string>(nullable: false),
                    CodigoMoneda = table.Column<string>(nullable: false),
                    CodigoProducto = table.Column<string>(nullable: true),
                    CodigoTransaccion = table.Column<string>(nullable: true),
                    RazonSocialCliente = table.Column<string>(nullable: false),
                    NroCuitCliente = table.Column<string>(nullable: false),
                    IdClienteOracle = table.Column<string>(nullable: false),
                    IdSiteOracle = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepeatedDebtDetails", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepeatedDebtDetails");
        }
    }
}
