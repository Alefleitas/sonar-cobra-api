using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class UpdateLockAdvancePayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedByDate",
                table: "LockAdvancePayment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LockedByDate",
                table: "LockAdvancePayment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
