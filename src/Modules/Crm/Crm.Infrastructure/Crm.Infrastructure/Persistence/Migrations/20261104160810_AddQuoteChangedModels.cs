using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteChangedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.AddColumn<string>(
                name: "CostCode",
                schema: "crm",
                table: "QuoteItemLines",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems",
                sql: "(\"Kind\" = 'OneOffPpe' AND \"ProductId\" IS NULL) OR \"Kind\" = 'CustomSewing' OR (\"Kind\" NOT IN ('OneOffPpe', 'CustomSewing') AND \"ProductId\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItemLines_Reference",
                schema: "crm",
                table: "QuoteItemLines",
                sql: "(\"Kind\" = 'Service' AND \"ReferenceId\" = 0 AND \"CostCode\" IS NOT NULL) OR (\"Kind\" <> 'Service' AND \"ReferenceId\" > 0 AND \"CostCode\" IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItemLines_Reference",
                schema: "crm",
                table: "QuoteItemLines");

            migrationBuilder.DropColumn(
                name: "CostCode",
                schema: "crm",
                table: "QuoteItemLines");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems",
                sql: "(\"Kind\" IN ('OneOffPpe', 'CustomSewing') AND \"ProductId\" IS NULL) OR (\"Kind\" NOT IN ('OneOffPpe', 'CustomSewing') AND \"ProductId\" IS NOT NULL)");
        }
    }
}
