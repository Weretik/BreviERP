using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "ProductPages",
                schema: "content",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ShowOnSite = table.Column<bool>(type: "boolean", nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPages", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "ProductPageTexts",
                schema: "content",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeoTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    SeoDescription = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Summary = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "jsonb", nullable: false),
                    Specs = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPageTexts", x => new { x.ProductId, x.Language });
                    table.ForeignKey(
                        name: "FK_ProductPageTexts_ProductPages_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "content",
                        principalTable: "ProductPages",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPages_ShowOnSite",
                schema: "content",
                table: "ProductPages",
                column: "ShowOnSite");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPages_Slug",
                schema: "content",
                table: "ProductPages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPageTexts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "ProductPages",
                schema: "content");
        }
    }
}
