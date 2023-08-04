using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddPaymentDetailAndRefactorPaymentMethodAndPaymenReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoOrganismo",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CreditingDate",
                table: "PaymentMethod");

            migrationBuilder.AddColumn<string>(
                name: "OlapAcuerdo",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentDetail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubOperationId = table.Column<string>(nullable: true),
                    Amount = table.Column<double>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Instrument = table.Column<string>(nullable: true),
                    ErrorDetail = table.Column<string>(nullable: true),
                    CreditingDate = table.Column<DateTime>(nullable: false),
                    PaymentMethodId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentDetail_PaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_PaymentMethodId",
                table: "PaymentDetail",
                column: "PaymentMethodId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "OlapAcuerdo",
                table: "PaymentMethod");

            migrationBuilder.AddColumn<string>(
                name: "CodigoOrganismo",
                table: "PaymentMethod",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreditingDate",
                table: "PaymentMethod",
                type: "datetime2",
                nullable: true);
        }
    }
}
