using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MatchLeadsByCompanyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_SourceId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.AddColumn<string>(
                name: "Warning",
                schema: "crm",
                table: "LeadSyncStates",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameKey",
                schema: "crm",
                table: "Leads",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Leads loaded before get the key Lead.KeyOf gives; a repeated name keeps the oldest lead as the match.
            migrationBuilder.Sql("""
                UPDATE crm."Leads" AS l SET "NameKey" = k.key
                FROM (
                    SELECT "Id",
                        CASE WHEN row_number() OVER (PARTITION BY key ORDER BY "Id") = 1 THEN key ELSE key || ' #' || "Id" END AS key
                    FROM (
                        SELECT "Id", left(btrim(regexp_replace(lower(translate("Company", '"''«»„“”‘’`', '          ')), '\s+', ' ', 'g')), 180) AS key
                        FROM crm."Leads") AS s) AS k
                WHERE l."Id" = k."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_NameKey",
                schema: "crm",
                table: "Leads",
                column: "NameKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_SourceId",
                schema: "crm",
                table: "Leads",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_NameKey",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_SourceId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Warning",
                schema: "crm",
                table: "LeadSyncStates");

            migrationBuilder.DropColumn(
                name: "NameKey",
                schema: "crm",
                table: "Leads");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_SourceId",
                schema: "crm",
                table: "Leads",
                column: "SourceId",
                unique: true);
        }
    }
}
