using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSewingCalculationArticleOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SewingCalculations_SewingArticles_ArticleId",
                schema: "catalog",
                table: "SewingCalculations");

            migrationBuilder.DropIndex(
                name: "IX_SewingCalculations_ArticleId",
                schema: "catalog",
                table: "SewingCalculations");

            migrationBuilder.AddForeignKey(
                name: "FK_SewingCalculations_SewingArticles_ProductId_ArticleId",
                schema: "catalog",
                table: "SewingCalculations",
                columns: new[] { "ProductId", "ArticleId" },
                principalSchema: "catalog",
                principalTable: "SewingArticles",
                principalColumns: new[] { "ProductId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SewingCalculations_SewingArticles_ProductId_ArticleId",
                schema: "catalog",
                table: "SewingCalculations");

            migrationBuilder.CreateIndex(
                name: "IX_SewingCalculations_ArticleId",
                schema: "catalog",
                table: "SewingCalculations",
                column: "ArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SewingCalculations_SewingArticles_ArticleId",
                schema: "catalog",
                table: "SewingCalculations",
                column: "ArticleId",
                principalSchema: "catalog",
                principalTable: "SewingArticles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
