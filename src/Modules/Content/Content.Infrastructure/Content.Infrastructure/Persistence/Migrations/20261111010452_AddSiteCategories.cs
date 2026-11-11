using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteCategories",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    ShowOnSite = table.Column<bool>(type: "boolean", nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteCategories_SiteCategories_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "content",
                        principalTable: "SiteCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiteCategoryProducts",
                schema: "content",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteCategoryProducts", x => new { x.CategoryId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_SiteCategoryProducts_SiteCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "content",
                        principalTable: "SiteCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteCategoryTexts",
                schema: "content",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeoTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Text = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteCategoryTexts", x => new { x.CategoryId, x.Language });
                    table.ForeignKey(
                        name: "FK_SiteCategoryTexts_SiteCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "content",
                        principalTable: "SiteCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteCategories_ParentId_SortOrder",
                schema: "content",
                table: "SiteCategories",
                columns: new[] { "ParentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteCategories_Slug",
                schema: "content",
                table: "SiteCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteCategoryProducts_ProductId",
                schema: "content",
                table: "SiteCategoryProducts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteCategoryProducts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "SiteCategoryTexts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "SiteCategories",
                schema: "content");
        }
    }
}
