using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductCategoryPathType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_ProductCategories_Path";
                """);

            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    index_name text;
                BEGIN
                    FOR index_name IN
                        SELECT indexname
                        FROM pg_indexes
                        WHERE tablename = 'ProductCategories'
                          AND indexdef ILIKE '%USING gist%'
                    LOOP
                        EXECUTE format('DROP INDEX IF EXISTS %I', index_name);
                    END LOOP;
                END $$;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "ProductCategories"
                ALTER COLUMN "Path" TYPE character varying(900)
                USING "Path"::text;
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ProductCategories_Path"
                ON "ProductCategories" ("Path");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "ProductCategories"
                ALTER COLUMN "Path" TYPE ltree
                USING "Path"::ltree;
                """);
        }
    }
}
