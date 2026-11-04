using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteHistoryAndArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                schema: "crm",
                table: "Quotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchivedByName",
                schema: "crm",
                table: "Quotes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuoteEvents",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ActorUserId = table.Column<int>(type: "integer", nullable: false),
                    ActorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteEvents_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ArchivedAt",
                schema: "crm",
                table: "Quotes",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteEvents_QuoteId_At",
                schema: "crm",
                table: "QuoteEvents",
                columns: new[] { "QuoteId", "At" });

            // Existing КП start their history with what is already known: who created them and every letter sent.
            migrationBuilder.Sql("""
                INSERT INTO crm."QuoteEvents" ("QuoteId", "Kind", "ActorUserId", "ActorName", "Details", "At")
                SELECT "Id", 'Created', "AuthorUserId", "AuthorName", NULL, "CreatedAt" FROM crm."Quotes";
                INSERT INTO crm."QuoteEvents" ("QuoteId", "Kind", "ActorUserId", "ActorName", "Details", "At")
                SELECT "QuoteId", 'Sent', "SenderUserId", "SenderName", LEFT('Кому: ' || "To", 500), "SentAt"
                FROM crm."QuoteSendings";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteEvents",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_ArchivedAt",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ArchivedByName",
                schema: "crm",
                table: "Quotes");
        }
    }
}
