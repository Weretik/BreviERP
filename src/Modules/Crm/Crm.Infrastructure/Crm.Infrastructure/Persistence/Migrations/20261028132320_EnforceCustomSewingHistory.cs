using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCustomSewingHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItemVariants_CustomVersion",
                schema: "crm",
                table: "QuoteItemVariants",
                sql: "(\"Kind\" = 'customSewing') = (\"CustomVersionId\" IS NOT NULL)");
            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_custom_history_change() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'custom_history_is_append_only' USING ERRCODE = '55000';
                END;
                $$;
                CREATE TRIGGER custom_version_append_only BEFORE UPDATE OR DELETE
                ON crm."CustomSewingVersions" FOR EACH ROW EXECUTE FUNCTION crm.reject_custom_history_change();
                CREATE FUNCTION crm.validate_custom_version_parent() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    IF (NEW."Number" = 1 AND NEW."ParentVersionId" IS NOT NULL)
                       OR (NEW."Number" > 1 AND NOT EXISTS (
                         SELECT 1 FROM crm."CustomSewingVersions" parent
                         WHERE parent."Id" = NEW."ParentVersionId" AND parent."DraftId" = NEW."DraftId"
                           AND parent."Number" = NEW."Number" - 1)) THEN
                        RAISE EXCEPTION 'invalid_custom_version_parent' USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END;
                $$;
                CREATE TRIGGER custom_version_parent BEFORE INSERT ON crm."CustomSewingVersions"
                FOR EACH ROW EXECUTE FUNCTION crm.validate_custom_version_parent();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER custom_version_parent ON crm."CustomSewingVersions";
                DROP FUNCTION crm.validate_custom_version_parent();
                DROP TRIGGER custom_version_append_only ON crm."CustomSewingVersions";
                DROP FUNCTION crm.reject_custom_history_change();
                """);
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItemVariants_CustomVersion",
                schema: "crm",
                table: "QuoteItemVariants");
        }
    }
}
