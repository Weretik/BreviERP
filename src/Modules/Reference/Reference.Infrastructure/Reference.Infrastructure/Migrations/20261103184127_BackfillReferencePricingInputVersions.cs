using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillReferencePricingInputVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fabrics, accessories and operations added before CAT-011 got no pricing input version, so Sewing
            // pricing could not see their prices. Each one without a version gets its first, approved revision.
            migrationBuilder.Sql(Backfill("Fabric", "FabricId", "FabricsReference", "\"Price\"", "1"));
            migrationBuilder.Sql(Backfill("Accessory", "AccessoryId", "GarmentAccessoriesReference", "\"Price\"",
                "CASE WHEN source.\"Unit\" = 'м' THEN 1 ELSE 2 END"));
            migrationBuilder.Sql(Backfill("Operation", "OperationId", "GarmentPartOperations", "\"Min\"", "4"));
        }

        private static string Backfill(string kind, string column, string table, string value, string unit) => $"""
            INSERT INTO reference."PricingInputVersions"
                ("{column}", "Value", "Unit", "IsActive", "IsApproved", "Revision", "EffectiveFrom",
                 "SourceCode", "SourceHash", "SourceDate")
            SELECT source."Id", source.{value}, {unit}, source."IsActive", TRUE, 1, now(),
                   'reference-backfill:{kind}:' || source."Id",
                   encode(sha256(convert_to('reference-backfill:{kind}:' || source."Id", 'UTF8')), 'hex'),
                   to_char(now() AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS"Z"')
            FROM reference."{table}" source
            WHERE NOT EXISTS (
                SELECT 1 FROM reference."PricingInputVersions" version WHERE version."{column}" = source."Id");
            """;

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM reference."PricingInputVersions" WHERE "SourceCode" LIKE 'reference-backfill:%';
                """);
        }
    }
}
