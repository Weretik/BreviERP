using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogProfileSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                schema: "catalog",
                table: "ProductCatalogProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCatalogProfiles_SectionId",
                schema: "catalog",
                table: "ProductCatalogProfiles",
                column: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductCatalogProfiles_SectionId",
                schema: "catalog",
                table: "ProductCatalogProfiles");

            migrationBuilder.DropColumn(
                name: "SectionId",
                schema: "catalog",
                table: "ProductCatalogProfiles");
        }
    }
}
