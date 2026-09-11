using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Photobiz.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddTenantCustomDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomDomain",
                table: "Tenants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomDomainVerifiedAt",
                table: "Tenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomainVerificationToken",
                table: "Tenants",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CustomDomain",
                table: "Tenants",
                column: "CustomDomain",
                unique: true,
                filter: "[CustomDomain] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_CustomDomain",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CustomDomain",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CustomDomainVerifiedAt",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DomainVerificationToken",
                table: "Tenants");
        }
    }
}
