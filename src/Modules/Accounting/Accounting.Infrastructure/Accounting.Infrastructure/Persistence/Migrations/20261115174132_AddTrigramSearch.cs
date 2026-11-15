using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Persistence.Migrations
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
                schema: "accounting",
                table: "Transfers",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Note\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "accounting",
                table: "Transactions",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Description\", '') || ' ' || coalesce(\"Comment\", '') || ' ' || coalesce(\"OneoffName\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SearchText",
                schema: "accounting",
                table: "Transfers",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SearchText",
                schema: "accounting",
                table: "Transactions",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_SearchText",
                schema: "accounting",
                table: "Counterparties",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transfers_SearchText",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SearchText",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Counterparties_SearchText",
                schema: "accounting",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "accounting",
                table: "Transactions");
            // pg_trgm stays: the search indexes of other modules use it.
        }
    }
}
