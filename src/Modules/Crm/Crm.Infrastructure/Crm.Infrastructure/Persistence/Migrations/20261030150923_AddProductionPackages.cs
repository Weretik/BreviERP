using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionPackageArtifacts",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    SelectionHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Bytes = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPackageArtifacts", x => x.Id);
                    table.UniqueConstraint("AK_ProductionPackageArtifacts_QuoteId_Id", x => new { x.QuoteId, x.Id });
                    table.CheckConstraint("CK_ProductionPackageArtifacts_Content", "octet_length(\"Bytes\") = \"Length\" AND \"Length\" > 0 AND \"Length\" <= 67108864");
                    table.CheckConstraint("CK_ProductionPackageArtifacts_Schema", "\"SchemaVersion\" = 1");
                    table.ForeignKey(
                        name: "FK_ProductionPackageArtifacts_QuoteVersions_QuoteId_QuoteVersi~",
                        columns: x => new { x.QuoteId, x.QuoteVersionId },
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPackageArtifacts_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionPackageRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    PackageArtifactId = table.Column<long>(type: "bigint", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPackageRequests", x => x.Id);
                    table.CheckConstraint("CK_ProductionPackageRequests_Actor", "\"ActorId\" > 0");
                    table.ForeignKey(
                        name: "FK_ProductionPackageRequests_ProductionPackageArtifacts_QuoteI~",
                        columns: x => new { x.QuoteId, x.PackageArtifactId },
                        principalSchema: "crm",
                        principalTable: "ProductionPackageArtifacts",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPackageRequests_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageArtifacts_PublicId",
                schema: "crm",
                table: "ProductionPackageArtifacts",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageArtifacts_QuoteId_QuoteVersionId_Selection~",
                schema: "crm",
                table: "ProductionPackageArtifacts",
                columns: new[] { "QuoteId", "QuoteVersionId", "SelectionHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageRequests_ActorId_RequestId",
                schema: "crm",
                table: "ProductionPackageRequests",
                columns: new[] { "ActorId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageRequests_QuoteId_PackageArtifactId",
                schema: "crm",
                table: "ProductionPackageRequests",
                columns: new[] { "QuoteId", "PackageArtifactId" });
            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_production_package_history_change() RETURNS trigger
                LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'Production package history is append-only';
                END;
                $$;
                CREATE TRIGGER production_package_artifacts_append_only
                    BEFORE UPDATE OR DELETE ON crm."ProductionPackageArtifacts"
                    FOR EACH ROW EXECUTE FUNCTION crm.reject_production_package_history_change();
                CREATE TRIGGER production_package_requests_append_only
                    BEFORE UPDATE OR DELETE ON crm."ProductionPackageRequests"
                    FOR EACH ROW EXECUTE FUNCTION crm.reject_production_package_history_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS production_package_requests_append_only ON crm."ProductionPackageRequests";
                DROP TRIGGER IF EXISTS production_package_artifacts_append_only ON crm."ProductionPackageArtifacts";
                DROP FUNCTION IF EXISTS crm.reject_production_package_history_change();
                """);
            migrationBuilder.DropTable(
                name: "ProductionPackageRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "ProductionPackageArtifacts",
                schema: "crm");
        }
    }
}
