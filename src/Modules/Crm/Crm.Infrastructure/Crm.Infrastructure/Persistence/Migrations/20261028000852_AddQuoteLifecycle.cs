using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_quote_history_change() RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'Quote lifecycle history is append only' USING ERRCODE = '23514';
                END;
                $$ LANGUAGE plpgsql;
                """);
            migrationBuilder.AddColumn<Guid>(
                name: "SourceQuotePublicId",
                schema: "crm",
                table: "Quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceVersionPublicId",
                schema: "crm",
                table: "Quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuoteLifecycleEvents",
                schema: "crm",
                columns: table => new
                {
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    PreviousOwnerId = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteLifecycleEvents", x => x.PublicId);
                    table.CheckConstraint("CK_QuoteLifecycleEvents_Revision", "\"Revision\" > 0 AND \"VersionNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_QuoteLifecycleEvents_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CreatedAt_Id",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_SourceQuotePublicId_Id",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "SourceQuotePublicId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLifecycleEvents_QuoteId_Revision",
                schema: "crm",
                table: "QuoteLifecycleEvents",
                columns: new[] { "QuoteId", "Revision" },
                unique: true);
            migrationBuilder.Sql("""
                CREATE TRIGGER quote_history_append_only BEFORE UPDATE OR DELETE
                ON crm."QuoteLifecycleEvents" FOR EACH ROW EXECUTE FUNCTION crm.reject_quote_history_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteLifecycleEvents",
                schema: "crm");
            migrationBuilder.Sql("DROP FUNCTION crm.reject_quote_history_change();");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_CreatedAt_Id",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_SourceQuotePublicId_Id",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "SourceQuotePublicId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "SourceVersionPublicId",
                schema: "crm",
                table: "Quotes");
        }
    }
}
