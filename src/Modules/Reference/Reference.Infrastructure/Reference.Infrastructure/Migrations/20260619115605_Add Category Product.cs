using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "ProductCategories" (
                    "Id" integer NOT NULL,
                    "Name" character varying(200) NOT NULL,
                    "RuName" character varying(200) NOT NULL,
                    "Slug" character varying(160) NOT NULL,
                    "Description" character varying(1000),
                    "ParentId" integer,
                    "Path" character varying(900) NOT NULL,
                    "Level" integer NOT NULL,
                    "SortOrder" integer NOT NULL,
                    "IsActive" boolean NOT NULL DEFAULT TRUE,
                    CONSTRAINT "PK_ProductCategories" PRIMARY KEY ("Id")
                );
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "ProductCategories"
                    ADD COLUMN IF NOT EXISTS "Name" character varying(200) NOT NULL,
                    ADD COLUMN IF NOT EXISTS "RuName" character varying(200) NOT NULL,
                    ADD COLUMN IF NOT EXISTS "Slug" character varying(160) NOT NULL,
                    ADD COLUMN IF NOT EXISTS "Description" character varying(1000),
                    ADD COLUMN IF NOT EXISTS "ParentId" integer,
                    ADD COLUMN IF NOT EXISTS "Path" character varying(900) NOT NULL,
                    ADD COLUMN IF NOT EXISTS "Level" integer NOT NULL,
                    ADD COLUMN IF NOT EXISTS "SortOrder" integer NOT NULL,
                    ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT TRUE;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_ProductCategories_ProductCategories_ParentId'
                    ) THEN
                        ALTER TABLE "ProductCategories"
                        ADD CONSTRAINT "FK_ProductCategories_ProductCategories_ParentId"
                        FOREIGN KEY ("ParentId")
                        REFERENCES "ProductCategories" ("Id")
                        ON DELETE RESTRICT;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ProductCategories_IsActive_SortOrder"
                ON "ProductCategories" ("IsActive", "SortOrder");
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ProductCategories_ParentId"
                ON "ProductCategories" ("ParentId");
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductCategories_ParentId_Slug"
                ON "ProductCategories" ("ParentId", "Slug");
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ProductCategories_Path"
                ON "ProductCategories" ("Path");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategories");
        }
    }
}
