using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddHistoricQuotations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoricQuotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Subtipo = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricQuotations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricQuotations");
        }
    }
}
