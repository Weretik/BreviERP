using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSewingPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SewingCalculations",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingCalculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SewingCalculations_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SewingCalculations_SewingArticles_ArticleId",
                        column: x => x.ArticleId,
                        principalSchema: "catalog",
                        principalTable: "SewingArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SewingPricePublications",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculationId = table.Column<long>(type: "bigint", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingPricePublications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SewingPricePublications_SewingCalculations_CalculationId",
                        column: x => x.CalculationId,
                        principalSchema: "catalog",
                        principalTable: "SewingCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SewingCalculations_ArticleId",
                schema: "catalog",
                table: "SewingCalculations",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_SewingCalculations_ProductId_ArticleId",
                schema: "catalog",
                table: "SewingCalculations",
                columns: new[] { "ProductId", "ArticleId" });

            migrationBuilder.CreateIndex(
                name: "IX_SewingCalculations_PublicId",
                schema: "catalog",
                table: "SewingCalculations",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SewingCalculations_RequestId",
                schema: "catalog",
                table: "SewingCalculations",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SewingPricePublications_CalculationId",
                schema: "catalog",
                table: "SewingPricePublications",
                column: "CalculationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SewingPricePublications_PublicId",
                schema: "catalog",
                table: "SewingPricePublications",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SewingPricePublications",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SewingCalculations",
                schema: "catalog");
        }
    }
}
