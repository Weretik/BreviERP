using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMovementSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "warehouse",
                table: "FabricMovements",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"SupplierName\", '') || ' ' || coalesce(\"Document\", '') || ' ' || coalesce(\"ModelName\", '') || ' ' || coalesce(\"Destination\", '') || ' ' || coalesce(\"Note\", '') || ' ' || coalesce(\"CreatedByName\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_SearchText",
                schema: "warehouse",
                table: "FabricMovements",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FabricMovements_SearchText",
                schema: "warehouse",
                table: "FabricMovements");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "warehouse",
                table: "FabricMovements");
            // pg_trgm stays: the search indexes of other modules use it.
        }
    }
}
