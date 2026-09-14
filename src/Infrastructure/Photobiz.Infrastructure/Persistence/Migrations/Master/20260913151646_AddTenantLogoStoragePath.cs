using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Photobiz.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddTenantLogoStoragePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoStoragePath",
                table: "Tenants",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoStoragePath",
                table: "Tenants");
        }
    }
}
