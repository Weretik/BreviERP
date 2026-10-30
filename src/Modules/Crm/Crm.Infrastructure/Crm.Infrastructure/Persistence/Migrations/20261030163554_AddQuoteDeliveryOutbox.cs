using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteDeliveryOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_QuotePdfArtifacts_QuoteId_QuoteVersionId_Id",
                schema: "crm",
                table: "QuotePdfArtifacts",
                columns: new[] { "QuoteId", "QuoteVersionId", "Id" });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveries",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    ArtifactId = table.Column<long>(type: "bigint", nullable: false),
                    EmailTemplateVersionId = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SenderAddress = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    RecipientAddress = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlainText = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    PdfBytes = table.Column<byte[]>(type: "bytea", nullable: false),
                    PdfSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveries", x => x.Id);
                    table.UniqueConstraint("AK_QuoteDeliveries_QuoteId_Id", x => new { x.QuoteId, x.Id });
                    table.UniqueConstraint("AK_QuoteDeliveries_QuoteId_QuoteVersionId_Id", x => new { x.QuoteId, x.QuoteVersionId, x.Id });
                    table.CheckConstraint("CK_QuoteDeliveries_Bytes", "octet_length(\"PdfBytes\") BETWEEN 5 AND 20971520");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveries_EmailTemplateVersions_EmailTemplateVersionId",
                        column: x => x.EmailTemplateVersionId,
                        principalSchema: "crm",
                        principalTable: "EmailTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteDeliveries_QuotePdfArtifacts_QuoteId_QuoteVersionId_Ar~",
                        columns: x => new { x.QuoteId, x.QuoteVersionId, x.ArtifactId },
                        principalSchema: "crm",
                        principalTable: "QuotePdfArtifacts",
                        principalColumns: new[] { "QuoteId", "QuoteVersionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteDeliveries_QuoteVersions_QuoteId_QuoteVersionId",
                        columns: x => new { x.QuoteId, x.QuoteVersionId },
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteDeliveries_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryEvents",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryEvents", x => x.Id);
                    table.CheckConstraint("CK_QuoteDeliveryEvents_Kind", "\"Kind\" IN ('queued','submit_started','reconcile_started','succeeded','failed','unknown','cancelled')");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryEvents_QuoteDeliveries_QuoteId_QuoteVersionId_~",
                        columns: x => new { x.QuoteId, x.QuoteVersionId, x.DeliveryId },
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumns: new[] { "QuoteId", "QuoteVersionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryRequests_QuoteDeliveries_QuoteId_DeliveryId",
                        columns: x => new { x.QuoteId, x.DeliveryId },
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryStates",
                schema: "crm",
                columns: table => new
                {
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextCheckAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryStates", x => x.DeliveryId);
                    table.CheckConstraint("CK_QuoteDeliveryStates_Attempt", "\"Attempt\" >= 0");
                    table.CheckConstraint("CK_QuoteDeliveryStates_Lease", "(\"Status\" IN ('sending','reconciling') AND \"LeaseToken\" IS NOT NULL AND \"LeaseUntil\" IS NOT NULL) OR (\"Status\" NOT IN ('sending','reconciling') AND \"LeaseToken\" IS NULL AND \"LeaseUntil\" IS NULL)");
                    table.CheckConstraint("CK_QuoteDeliveryStates_Status", "\"Status\" IN ('queued','sending','reconciling','unknown','succeeded','failed')");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryStates_QuoteDeliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_EmailTemplateVersionId",
                schema: "crm",
                table: "QuoteDeliveries",
                column: "EmailTemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_PublicId",
                schema: "crm",
                table: "QuoteDeliveries",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_QuoteId_CreatedAt",
                schema: "crm",
                table: "QuoteDeliveries",
                columns: new[] { "QuoteId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_QuoteId_QuoteVersionId_ArtifactId",
                schema: "crm",
                table: "QuoteDeliveries",
                columns: new[] { "QuoteId", "QuoteVersionId", "ArtifactId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryEvents_DeliveryId_CreatedAt",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                columns: new[] { "DeliveryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryEvents_DeliveryId_Kind",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                columns: new[] { "DeliveryId", "Kind" },
                unique: true,
                filter: "\"Kind\" = 'succeeded'");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryEvents_QuoteId_QuoteVersionId_DeliveryId",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                columns: new[] { "QuoteId", "QuoteVersionId", "DeliveryId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryRequests_ActorId_RequestId",
                schema: "crm",
                table: "QuoteDeliveryRequests",
                columns: new[] { "ActorId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryRequests_QuoteId_DeliveryId",
                schema: "crm",
                table: "QuoteDeliveryRequests",
                columns: new[] { "QuoteId", "DeliveryId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryStates_Status_LeaseUntil_NextCheckAt_UpdatedAt",
                schema: "crm",
                table: "QuoteDeliveryStates",
                columns: new[] { "Status", "LeaseUntil", "NextCheckAt", "UpdatedAt" });
            migrationBuilder.Sql("""
                CREATE FUNCTION crm.protect_quote_delivery_evidence() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN RAISE EXCEPTION 'Quote delivery evidence is immutable'; END $$;
                CREATE TRIGGER protect_quote_deliveries BEFORE UPDATE OR DELETE ON crm."QuoteDeliveries"
                FOR EACH ROW EXECUTE FUNCTION crm.protect_quote_delivery_evidence();
                CREATE TRIGGER protect_quote_delivery_events BEFORE UPDATE OR DELETE ON crm."QuoteDeliveryEvents"
                FOR EACH ROW EXECUTE FUNCTION crm.protect_quote_delivery_evidence();
                CREATE TRIGGER protect_quote_delivery_requests BEFORE UPDATE OR DELETE ON crm."QuoteDeliveryRequests"
                FOR EACH ROW EXECUTE FUNCTION crm.protect_quote_delivery_evidence();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("""
                DROP TRIGGER protect_quote_deliveries ON crm."QuoteDeliveries";
                DROP TRIGGER protect_quote_delivery_events ON crm."QuoteDeliveryEvents";
                DROP TRIGGER protect_quote_delivery_requests ON crm."QuoteDeliveryRequests";
                DROP FUNCTION crm.protect_quote_delivery_evidence();
                """);
            migrationBuilder.DropTable(
                name: "QuoteDeliveryEvents",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveryRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveryStates",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveries",
                schema: "crm");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_QuotePdfArtifacts_QuoteId_QuoteVersionId_Id",
                schema: "crm",
                table: "QuotePdfArtifacts");
        }
    }
}
