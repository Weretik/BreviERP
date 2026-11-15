using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
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
                schema: "crm",
                table: "Quotes",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Number\", '') || ' ' || coalesce(\"InternalName\", '') || ' ' || coalesce(\"CompanyName\", '') || ' ' || coalesce(\"RecipientName\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "crm",
                table: "ManagerTasks",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Title\", '') || ' ' || coalesce(\"CustomerName\", '') || ' ' || coalesce(\"AssigneeName\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "crm",
                table: "Leads",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Company\", '') || ' ' || coalesce(\"Region\", '') || ' ' || coalesce(\"Industry\", '') || ' ' || coalesce(\"Contacts\", '') || ' ' || coalesce(\"OwnerName\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "crm",
                table: "EmbroideryCalculations",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"Name\", '') || ' ' || coalesce(\"CustomerName\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "crm",
                table: "Customers",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"NormalizedName\", '') || ' ' || coalesce(\"TaxIdentity\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "crm",
                table: "ContactChannels",
                type: "text",
                nullable: true,
                computedColumnSql: "replace(replace(lower(coalesce(\"NormalizedValue\", '')), '’', ''''), 'ʼ', '''')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_SearchText",
                schema: "crm",
                table: "Quotes",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTasks_SearchText",
                schema: "crm",
                table: "ManagerTasks",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_SearchText",
                schema: "crm",
                table: "Leads",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_EmbroideryCalculations_SearchText",
                schema: "crm",
                table: "EmbroideryCalculations",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SearchText",
                schema: "crm",
                table: "Customers",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactChannels_SearchText",
                schema: "crm",
                table: "ContactChannels",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quotes_SearchText",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_ManagerTasks_SearchText",
                schema: "crm",
                table: "ManagerTasks");

            migrationBuilder.DropIndex(
                name: "IX_Leads_SearchText",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_EmbroideryCalculations_SearchText",
                schema: "crm",
                table: "EmbroideryCalculations");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SearchText",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_ContactChannels_SearchText",
                schema: "crm",
                table: "ContactChannels");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "crm",
                table: "ManagerTasks");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "crm",
                table: "EmbroideryCalculations");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "crm",
                table: "ContactChannels");
            // pg_trgm stays: the search indexes of other modules use it.
        }
    }
}
