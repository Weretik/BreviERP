using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteRedirects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteRedirects",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ToPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteRedirects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteRedirects_FromPath",
                schema: "content",
                table: "SiteRedirects",
                column: "FromPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteRedirects_ToPath",
                schema: "content",
                table: "SiteRedirects",
                column: "ToPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteRedirects",
                schema: "content");
        }
    }
}
