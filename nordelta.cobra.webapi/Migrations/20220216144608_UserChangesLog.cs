using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class UserChangesLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedPublishDebtBy",
                table: "AccountBalances");

            migrationBuilder.CreateTable(
                name: "UserChangesLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(nullable: false),
                    ModifiedEntity = table.Column<string>(nullable: true),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ModifiedField = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    UserEmail = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChangesLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserChangesLog_Id_IsDeleted",
                table: "UserChangesLog",
                columns: new[] { "Id", "IsDeleted" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChangesLog");

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedPublishDebtBy",
                table: "AccountBalances",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
