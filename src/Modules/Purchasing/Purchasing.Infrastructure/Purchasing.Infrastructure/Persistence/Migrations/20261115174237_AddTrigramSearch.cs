using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Purchasing.Infrastructure.Persistence.Migrations
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
                schema: "purchasing",
                table: "Messages",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Text\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SearchText",
                schema: "purchasing",
                table: "Messages",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_SearchText",
                schema: "purchasing",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "purchasing",
                table: "Messages");
            // pg_trgm stays: the search indexes of other modules use it.
        }
    }
}
