using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class SeparateBalanceFromDelayStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelayStatus",
                table: "AccountBalances",
                nullable: true);
            migrationBuilder.Sql(
               "UPDATE AccountBalances SET " +
               "DelayStatus = CASE Balance " +
                       "WHEN 2 THEN 0 " +
                       "WHEN 3 THEN 1 " +
                       "WHEN 4 THEN 2 " + 
                       "WHEN 5 THEN 3 " +
                       "ELSE NULL END, " +
                "Balance = CASE Balance " +
                       "WHEN 2 THEN 1 " +
                       "WHEN 3 THEN 1 " +
                       "WHEN 4 THEN 1 " +
                       "WHEN 5 THEN 1 " +
                       "ELSE Balance END ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
              "UPDATE AccountBalances SET " +
               "Balance = CASE DelayStatus " +
                      "WHEN 0 THEN 2 " +
                      "WHEN 1 THEN 3 " +
                      "WHEN 2 THEN 4 " +
                      "WHEN 3 THEN 5 " +
                      "ELSE Balance END ");
            migrationBuilder.DropColumn(
                name: "DelayStatus",
                table: "AccountBalances");
        }
    }
}
