using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrigramSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "catalog",
                table: "Products",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Name\", '') || ' ' || coalesce(\"RuName\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SearchText",
                schema: "catalog",
                table: "Products",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SearchText",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "catalog",
                table: "Products");
            // pg_trgm stays: the search indexes of other modules use it.
        }
    }
}
