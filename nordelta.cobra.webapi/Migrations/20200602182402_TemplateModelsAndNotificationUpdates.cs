using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class TemplateModelsAndNotificationUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "HtmlFileBase",
                table: "Template",
                newName: "Subject");

            migrationBuilder.AddColumn<string>(
                name: "HtmlBody",
                table: "Template",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TemplateTokenReference",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: false),
                    Token = table.Column<string>(nullable: false),
                    ObjectProperty = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTokenReference", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateTokenReference");

            migrationBuilder.DropColumn(
                name: "HtmlBody",
                table: "Template");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "Template",
                newName: "HtmlFileBase");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Notification",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Notification",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ContactDetails",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Communications",
                nullable: false,
                defaultValue: "");
        }
    }
}
