using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class InsertProductCodeAndSignatureTokenReferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TemplateTokenReference",
                columns: new[] { "Description", "Token", "ObjectProperty" },
                values: new object[] { "Código de producto", "{{CODIGO_PRODUCTO}}", "CodProducto" }
                );
            migrationBuilder.InsertData(
                table: "TemplateTokenReference",
                columns: new[] { "Description", "Token", "ObjectProperty" },
                values: new object[] { "Firma correspondiente a la BU", "{{FIRMA_BU}}", "FirmaBU" }
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
            table: "TemplateTokenReference",
            keyColumn: "Token",
            keyValue: "{{CODIGO_PRODUCTO}}");
            migrationBuilder.DeleteData(
            table: "TemplateTokenReference",
            keyColumn: "Token",
            keyValue: "{{FIRMA_BU}}");
        }
    }
}
