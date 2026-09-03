using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Photobiz.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAdminUserToUserWithRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Galleries_AdminUsers_AdminUserId",
                table: "Galleries");

            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.RenameColumn(
                name: "AdminUserId",
                table: "Galleries",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Galleries_AdminUserId",
                table: "Galleries",
                newName: "IX_Galleries_UserId");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Galleries_Users_UserId",
                table: "Galleries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Galleries_Users_UserId",
                table: "Galleries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Galleries",
                newName: "AdminUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Galleries_UserId",
                table: "Galleries",
                newName: "IX_Galleries_AdminUserId");

            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Galleries_AdminUsers_AdminUserId",
                table: "Galleries",
                column: "AdminUserId",
                principalTable: "AdminUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
