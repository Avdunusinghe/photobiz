using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Photobiz.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CustomerFirstName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CustomerLastName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    City = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BillingCycle = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayHerePaymentId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SubscriptionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PayhereAmount = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PayhereCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Method = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Recurring = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    ItemRecurrence = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ItemDuration = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ItemRecStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ItemRecDateNext = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ItemRecInstallPaid = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    CardHolderName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CardNo = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CardExpiry = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    IsSubscribed = table.Column<bool>(type: "bit", nullable: false),
                    SubscriptionExpiredOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TenantKey",
                table: "Tenants",
                column: "TenantKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
