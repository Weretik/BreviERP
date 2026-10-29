using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteArtifactIssuePin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_QuoteIssues_QuoteId_QuoteVersionId_IssuerVersionId_Template~",
                schema: "crm",
                table: "QuoteIssues",
                columns: new[] { "QuoteId", "QuoteVersionId", "IssuerVersionId", "TemplateVersionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_QuotePdfArtifacts_QuoteIssues_QuoteId_QuoteVersionId_Issuer~",
                schema: "crm",
                table: "QuotePdfArtifacts",
                columns: new[] { "QuoteId", "QuoteVersionId", "IssuerVersionId", "TemplateVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteIssues",
                principalColumns: new[] { "QuoteId", "QuoteVersionId", "IssuerVersionId", "TemplateVersionId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_quote_issue_change() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'quote_issue_is_append_only' USING ERRCODE = '55000';
                END;
                $$;
                CREATE TRIGGER quote_issue_append_only BEFORE UPDATE OR DELETE
                ON crm."QuoteIssues" FOR EACH ROW EXECUTE FUNCTION crm.reject_quote_issue_change();

                CREATE FUNCTION crm.reject_succeeded_quote_pdf_change() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    IF OLD."Status" = 'succeeded' THEN
                        RAISE EXCEPTION 'successful_quote_pdf_is_immutable' USING ERRCODE = '55000';
                    END IF;
                    IF TG_OP = 'DELETE' THEN RETURN OLD; END IF;
                    RETURN NEW;
                END;
                $$;
                CREATE TRIGGER quote_pdf_succeeded_immutable BEFORE UPDATE OR DELETE
                ON crm."QuotePdfArtifacts" FOR EACH ROW EXECUTE FUNCTION crm.reject_succeeded_quote_pdf_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER quote_pdf_succeeded_immutable ON crm."QuotePdfArtifacts";
                DROP FUNCTION crm.reject_succeeded_quote_pdf_change();
                DROP TRIGGER quote_issue_append_only ON crm."QuoteIssues";
                DROP FUNCTION crm.reject_quote_issue_change();
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_QuotePdfArtifacts_QuoteIssues_QuoteId_QuoteVersionId_Issuer~",
                schema: "crm",
                table: "QuotePdfArtifacts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_QuoteIssues_QuoteId_QuoteVersionId_IssuerVersionId_Template~",
                schema: "crm",
                table: "QuoteIssues");
        }
    }
}
