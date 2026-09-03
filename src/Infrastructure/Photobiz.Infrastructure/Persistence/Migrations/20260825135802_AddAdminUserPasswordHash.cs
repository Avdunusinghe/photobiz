using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Photobiz.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "AdminUsers",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "AdminUsers");
        }
    }
}
