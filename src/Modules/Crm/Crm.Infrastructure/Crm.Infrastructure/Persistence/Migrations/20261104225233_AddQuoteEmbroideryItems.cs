using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteEmbroideryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.AddColumn<long>(
                name: "EmbroideryCalculationId",
                schema: "crm",
                table: "QuoteItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_EmbroideryCalculationId",
                schema: "crm",
                table: "QuoteItems",
                column: "EmbroideryCalculationId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItems_Embroidery",
                schema: "crm",
                table: "QuoteItems",
                sql: "(\"Kind\" = 'Embroidery') = (\"EmbroideryCalculationId\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems",
                sql: "(\"Kind\" IN ('OneOffPpe', 'Embroidery') AND \"ProductId\" IS NULL) OR \"Kind\" = 'CustomSewing' OR (\"Kind\" NOT IN ('OneOffPpe', 'CustomSewing', 'Embroidery') AND \"ProductId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuoteItems_EmbroideryCalculationId",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItems_Embroidery",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "EmbroideryCalculationId",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems",
                sql: "(\"Kind\" = 'OneOffPpe' AND \"ProductId\" IS NULL) OR \"Kind\" = 'CustomSewing' OR (\"Kind\" NOT IN ('OneOffPpe', 'CustomSewing') AND \"ProductId\" IS NOT NULL)");
        }
    }
}
