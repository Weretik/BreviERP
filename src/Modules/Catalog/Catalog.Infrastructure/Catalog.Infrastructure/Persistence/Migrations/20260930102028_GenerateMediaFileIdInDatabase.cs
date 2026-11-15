using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GenerateMediaFileIdInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "catalog",
                table: "MediaFiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // A DO block, so the same statement also runs inside the DO block of an idempotent script, where a bare
            // SELECT has no destination. The effect is unchanged for databases that already applied it.
            migrationBuilder.Sql("""
                DO $setval$
                BEGIN
                    PERFORM setval(
                        pg_get_serial_sequence('catalog."MediaFiles"', 'Id'),
                        COALESCE((SELECT MAX("Id")::bigint FROM catalog."MediaFiles"), 0) + 1,
                        false);
                END $setval$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "catalog",
                table: "MediaFiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
