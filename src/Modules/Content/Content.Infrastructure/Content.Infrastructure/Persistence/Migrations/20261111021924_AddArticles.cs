using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShowOnSite = table.Column<bool>(type: "boolean", nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PublishedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    CoverPhotoId = table.Column<int>(type: "integer", nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTexts",
                schema: "content",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Excerpt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SeoTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Body = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTexts", x => new { x.ArticleId, x.Language });
                    table.ForeignKey(
                        name: "FK_ArticleTexts_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalSchema: "content",
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PublishedOn",
                schema: "content",
                table: "Articles",
                column: "PublishedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Slug",
                schema: "content",
                table: "Articles",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleTexts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Articles",
                schema: "content");
        }
    }
}
