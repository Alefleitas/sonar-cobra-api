using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddAdvanceFeeOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdvanceFees",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodProducto = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    ClientCuit = table.Column<long>(nullable: false),
                    Vencimiento = table.Column<DateTime>(nullable: false),
                    Moneda = table.Column<int>(nullable: false),
                    Importe = table.Column<float>(nullable: false),
                    Saldo = table.Column<float>(nullable: false),
                    Informed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceFees", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvanceFees");
        }
    }
}
