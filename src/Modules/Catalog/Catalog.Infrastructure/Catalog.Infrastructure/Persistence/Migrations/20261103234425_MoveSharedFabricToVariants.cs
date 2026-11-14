using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <summary>
    /// CAT-015: the fabric belongs to the fabric variant (Article). A model whose only fabric line is shared and whose
    /// variants have no own fabric gets that line copied to every variant; the shared line is removed. The price stays
    /// the same because every variant used that fabric already.
    /// </summary>
    public partial class MoveSharedFabricToVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TEMP TABLE moved_shared_fabric AS
                SELECT l."Id", l."ProductId"
                FROM catalog."SewingMaterialLines" l
                WHERE l."ArticleId" IS NULL AND l."Kind" = 1
                  AND (SELECT count(*) FROM catalog."SewingMaterialLines" s
                       WHERE s."ProductId" = l."ProductId" AND s."ArticleId" IS NULL AND s."Kind" = 1) = 1
                  AND NOT EXISTS (SELECT 1 FROM catalog."SewingMaterialLines" a
                                  WHERE a."ProductId" = l."ProductId" AND a."ArticleId" IS NOT NULL)
                  AND EXISTS (SELECT 1 FROM catalog."SewingArticles" ar WHERE ar."ProductId" = l."ProductId");

                INSERT INTO catalog."SewingMaterialLines"
                    ("ProductId", "ArticleId", "Kind", "FabricId", "AccessoryId", "CostCode", "Quantity", "Unit", "SortOrder", "Comment")
                SELECT l."ProductId", ar."Id", l."Kind", l."FabricId", NULL, NULL, l."Quantity", l."Unit", 0, l."Comment"
                FROM moved_shared_fabric m
                JOIN catalog."SewingMaterialLines" l ON l."Id" = m."Id"
                JOIN catalog."SewingArticles" ar ON ar."ProductId" = l."ProductId";

                DELETE FROM catalog."SewingMaterialLines" WHERE "Id" IN (SELECT "Id" FROM moved_shared_fabric);

                UPDATE catalog."Products" SET "CompositionVersion" = "CompositionVersion" + 1
                WHERE "Id" IN (SELECT "ProductId" FROM moved_shared_fabric);

                DROP TABLE moved_shared_fabric;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data move only; the schema is unchanged. Moving the fabric back is not needed for a rollback.
        }
    }
}
