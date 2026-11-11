using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSeoSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSeoSettings",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GoogleAnalyticsId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GoogleTagManagerId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GoogleSiteVerification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BingSiteVerification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultOgPhotoId = table.Column<int>(type: "integer", nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSeoSettings", x => x.Id);
                    table.CheckConstraint("CK_SiteSeoSettings_Single", "\"Id\" = 1");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSeoSettings",
                schema: "content");
        }
    }
}
