using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteDeliveryManualResolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteDeliveryEvents_Kind",
                schema: "crm",
                table: "QuoteDeliveryEvents");

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryResolutions",
                schema: "crm",
                columns: table => new
                {
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EvidenceNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryResolutions", x => x.DeliveryId);
                    table.CheckConstraint("CK_QuoteDeliveryResolutions_ActorAttempt", "\"ActorId\" > 0 AND \"Attempt\" > 0");
                    table.CheckConstraint("CK_QuoteDeliveryResolutions_Evidence", "length(btrim(\"EvidenceNote\")) > 0");
                    table.CheckConstraint("CK_QuoteDeliveryResolutions_Outcome", "\"Outcome\" IN ('sent','not_sent')");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryResolutions_QuoteDeliveries_QuoteId_DeliveryId",
                        columns: x => new { x.QuoteId, x.DeliveryId },
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteDeliveryEvents_Kind",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                sql: "\"Kind\" IN ('queued','submit_started','reconcile_started','succeeded','failed','unknown','cancelled','manual_succeeded','manual_failed')");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryResolutions_QuoteId_DeliveryId",
                schema: "crm",
                table: "QuoteDeliveryResolutions",
                columns: new[] { "QuoteId", "DeliveryId" });
            migrationBuilder.Sql("""
                CREATE TRIGGER protect_quote_delivery_resolutions BEFORE UPDATE OR DELETE ON crm."QuoteDeliveryResolutions"
                FOR EACH ROW EXECUTE FUNCTION crm.protect_quote_delivery_evidence();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER protect_quote_delivery_resolutions ON crm.\"QuoteDeliveryResolutions\"");
            migrationBuilder.DropTable(
                name: "QuoteDeliveryResolutions",
                schema: "crm");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteDeliveryEvents_Kind",
                schema: "crm",
                table: "QuoteDeliveryEvents");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteDeliveryEvents_Kind",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                sql: "\"Kind\" IN ('queued','submit_started','reconcile_started','succeeded','failed','unknown','cancelled')");
        }
    }
}
