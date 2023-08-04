using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class RemoveNroCuitClienteFromUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DetallesDeuda_ArchivoDeudaId_NroComprobante_FechaPrimerVenc_CodigoMoneda_ObsLibreSegunda_NroCuitCliente",
                table: "DetallesDeuda");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_ArchivoDeudaId_NroComprobante_FechaPrimerVenc_CodigoMoneda_ObsLibreSegunda",
                table: "DetallesDeuda",
                columns: new[] { "ArchivoDeudaId", "NroComprobante", "FechaPrimerVenc", "CodigoMoneda", "ObsLibreSegunda" },
                unique: true,
                filter: "[NroComprobante] IS NOT NULL AND [FechaPrimerVenc] IS NOT NULL AND [CodigoMoneda] IS NOT NULL AND [ObsLibreSegunda] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DetallesDeuda_ArchivoDeudaId_NroComprobante_FechaPrimerVenc_CodigoMoneda_ObsLibreSegunda",
                table: "DetallesDeuda");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDeuda_ArchivoDeudaId_NroComprobante_FechaPrimerVenc_CodigoMoneda_ObsLibreSegunda_NroCuitCliente",
                table: "DetallesDeuda",
                columns: new[] { "ArchivoDeudaId", "NroComprobante", "FechaPrimerVenc", "CodigoMoneda", "ObsLibreSegunda", "NroCuitCliente" },
                unique: true,
                filter: "[NroComprobante] IS NOT NULL AND [FechaPrimerVenc] IS NOT NULL AND [CodigoMoneda] IS NOT NULL AND [ObsLibreSegunda] IS NOT NULL AND [NroCuitCliente] IS NOT NULL");
        }
    }
}
