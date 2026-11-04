using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteSending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSentAt",
                schema: "crm",
                table: "Quotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuoteSendings",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersion = table.Column<int>(type: "integer", nullable: false),
                    SenderUserId = table.Column<int>(type: "integer", nullable: false),
                    SenderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FromEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    To = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteSendings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteSendings_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffMailboxes",
                schema: "crm",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    ProtectedRefreshToken = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ConnectedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConnectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMailboxes", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteSendings_QuoteId_SentAt",
                schema: "crm",
                table: "QuoteSendings",
                columns: new[] { "QuoteId", "SentAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteSendings",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "StaffMailboxes",
                schema: "crm");

            migrationBuilder.DropColumn(
                name: "LastSentAt",
                schema: "crm",
                table: "Quotes");
        }
    }
}
