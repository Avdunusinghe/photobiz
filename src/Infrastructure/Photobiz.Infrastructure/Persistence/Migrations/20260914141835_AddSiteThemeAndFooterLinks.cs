using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Photobiz.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteThemeAndFooterLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Template",
                table: "Galleries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SiteThemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    GradientStartColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    GradientEndColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    GradientDirection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FontFamily = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    HeaderStyle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tagline = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    FooterText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FooterCopyrightText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultGalleryTemplate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteThemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteFooterLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteFooterLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteFooterLinks_SiteThemes_SiteThemeId",
                        column: x => x.SiteThemeId,
                        principalTable: "SiteThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteFooterLinks_SiteThemeId",
                table: "SiteFooterLinks",
                column: "SiteThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteFooterLinks");

            migrationBuilder.DropTable(
                name: "SiteThemes");

            migrationBuilder.DropColumn(
                name: "Template",
                table: "Galleries");
        }
    }
}
