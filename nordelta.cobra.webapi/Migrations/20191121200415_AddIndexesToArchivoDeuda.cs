using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddIndexesToArchivoDeuda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FormatedFileName",
                table: "ArchivosDeuda",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ArchivosDeuda",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosDeuda_FileName",
                table: "ArchivosDeuda",
                column: "FileName",
                unique: true,
                filter: "[FileName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosDeuda_FormatedFileName",
                table: "ArchivosDeuda",
                column: "FormatedFileName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArchivosDeuda_FileName",
                table: "ArchivosDeuda");

            migrationBuilder.DropIndex(
                name: "IX_ArchivosDeuda_FormatedFileName",
                table: "ArchivosDeuda");

            migrationBuilder.AlterColumn<string>(
                name: "FormatedFileName",
                table: "ArchivosDeuda",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ArchivosDeuda",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
