using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddCvuEntityAndPaymentMethodRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("DebinId", "DetallesDeuda", "PaymentMethodId", "dbo");

            migrationBuilder.AddColumn<string>(
                name: "CoelsaId",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CvuEntityId",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CvuOperationStatus",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationId",
                table: "PaymentMethod",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CvuEntities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItauCreationTransactionId = table.Column<string>(nullable: false),
                    CvuValue = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Currency = table.Column<int>(nullable: false),
                    AccountBalanceId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvuEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CvuEntities_AccountBalances_AccountBalanceId",
                        column: x => x.AccountBalanceId,
                        principalTable: "AccountBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_CvuEntityId",
                table: "PaymentMethod",
                column: "CvuEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CvuEntities_AccountBalanceId",
                table: "CvuEntities",
                column: "AccountBalanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethod_CvuEntities_CvuEntityId",
                table: "PaymentMethod",
                column: "CvuEntityId",
                principalTable: "CvuEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("PaymentMethodId", "DetallesDeuda", "DebinId", "dbo");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethod_CvuEntities_CvuEntityId",
                table: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "CvuEntities");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethod_CvuEntityId",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CoelsaId",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CvuEntityId",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CvuOperationStatus",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "PaymentMethod");

        }
    }
}
