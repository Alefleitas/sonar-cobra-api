using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddAnonymousPaymentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnonymousPayment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Cuit = table.Column<string>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    Currency = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    TransactionDate = table.Column<DateTime>(nullable: false),
                    IssueDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    CBU = table.Column<string>(nullable: false),
                    VendorCuit = table.Column<string>(nullable: true),
                    System = table.Column<string>(nullable: false),
                    Migrated = table.Column<bool>(nullable: false),
                    DebinCode = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnonymousPayment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousPayment_Id_IsDeleted",
                table: "AnonymousPayment",
                columns: new[] { "Id", "IsDeleted" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnonymousPayment");
        }
    }
}
