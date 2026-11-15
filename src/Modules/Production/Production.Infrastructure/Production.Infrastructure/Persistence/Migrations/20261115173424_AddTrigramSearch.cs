using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
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
                schema: "production",
                table: "Orders",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"ClientName\", '') || ' ' || coalesce(\"QuoteNumber\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "production",
                table: "Items",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"ModelName\", '') || ' ' || coalesce(\"ModelCode\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SearchText",
                schema: "production",
                table: "Orders",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_SearchText",
                schema: "production",
                table: "Items",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_SearchText",
                schema: "production",
                table: "ArchiveRecords",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_SearchText",
                schema: "production",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Items_SearchText",
                schema: "production",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_ArchiveRecords_SearchText",
                schema: "production",
                table: "ArchiveRecords");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "production",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "production",
                table: "Items");

            // pg_trgm stays: the search indexes of other modules use it.
        }
    }
}
