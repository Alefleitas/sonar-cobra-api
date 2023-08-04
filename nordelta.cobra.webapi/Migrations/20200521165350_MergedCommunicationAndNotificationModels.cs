using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class MergedCommunicationAndNotificationModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Balance",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "AccountBalances",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ContactStatus",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FuturePaymentsAmountUSD",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "FuturePaymentsCount",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OverduePaymentDate",
                table: "AccountBalances",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OverduePaymentsAmountUSD",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "OverduePaymentsCount",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PaidPaymentsAmountUSD",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PaidPaymentsCount",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "AccountBalances",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TotalDebtAmount",
                table: "AccountBalances",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "ContactDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ComChannel = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Configuration = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Template",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    HtmlFileBase = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Template", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Communications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountBalanceId = table.Column<int>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    NextCommunicationDate = table.Column<DateTime>(nullable: true),
                    Incoming = table.Column<bool>(nullable: false),
                    Client = table.Column<string>(nullable: false),
                    CommunicationChannel = table.Column<int>(nullable: false),
                    CommunicationResult = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    ContactDetailId = table.Column<int>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedBy = table.Column<string>(nullable: true),
                    LastModifiedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Communications_AccountBalances_AccountBalanceId",
                        column: x => x.AccountBalanceId,
                        principalTable: "AccountBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Communications_ContactDetails_ContactDetailId",
                        column: x => x.ContactDetailId,
                        principalTable: "ContactDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeliveryId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    TemplateId = table.Column<int>(nullable: true),
                    ConfigurationDays = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationType_DeliveryType_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "DeliveryType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationType_Template_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    NotificationTypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalTable: "NotificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTypeXRole",
                columns: table => new
                {
                    RoleId = table.Column<string>(nullable: false),
                    NotificationTypeId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypeXRole", x => new { x.NotificationTypeId, x.RoleId });
                    table.UniqueConstraint("AK_NotificationTypeXRole_Id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTypeXRole_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalTable: "NotificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationTypeXRole_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationXUser",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    User = table.Column<string>(nullable: false),
                    NotificationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationXUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationXUser_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Communications_AccountBalanceId",
                table: "Communications",
                column: "AccountBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Communications_Client",
                table: "Communications",
                column: "Client");

            migrationBuilder.CreateIndex(
                name: "IX_Communications_ContactDetailId",
                table: "Communications",
                column: "ContactDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Communications_Id_IsDeleted",
                table: "Communications",
                columns: new[] { "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_NotificationTypeId",
                table: "Notification",
                column: "NotificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationType_DeliveryId",
                table: "NotificationType",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationType_TemplateId",
                table: "NotificationType",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTypeXRole_RoleId",
                table: "NotificationTypeXRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationXUser_NotificationId",
                table: "NotificationXUser",
                column: "NotificationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Communications");

            migrationBuilder.DropTable(
                name: "NotificationTypeXRole");

            migrationBuilder.DropTable(
                name: "NotificationXUser");

            migrationBuilder.DropTable(
                name: "ContactDetails");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "NotificationType");

            migrationBuilder.DropTable(
                name: "DeliveryType");

            migrationBuilder.DropTable(
                name: "Template");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "ContactStatus",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "FuturePaymentsAmountUSD",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "FuturePaymentsCount",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "OverduePaymentDate",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "OverduePaymentsAmountUSD",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "OverduePaymentsCount",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "PaidPaymentsAmountUSD",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "PaidPaymentsCount",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "Product",
                table: "AccountBalances");

            migrationBuilder.DropColumn(
                name: "TotalDebtAmount",
                table: "AccountBalances");
        }
    }
}
