using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddFieldAutoApprovedInAdvanceFee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoApproved",
                table: "AdvanceFees",
                type: "bit",
                nullable: true,
                defaultValue: false
                );

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {        

            migrationBuilder.DropColumn(
                name: "AutoApproved",
                table: "AdvanceFees");

         
        }
    }
}
