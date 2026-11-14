using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistDataProtectionKeysAndDropRefreshSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRefreshSessions",
                schema: "identity");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys",
                schema: "identity");

            migrationBuilder.CreateTable(
                name: "AppRefreshSessions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AbsoluteExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastUsedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReplacedBySessionId = table.Column<int>(type: "integer", nullable: true),
                    ReuseDetectedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRefreshSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRefreshSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshSessions_AbsoluteExpiresAtUtc",
                schema: "identity",
                table: "AppRefreshSessions",
                column: "AbsoluteExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshSessions_TokenHash",
                schema: "identity",
                table: "AppRefreshSessions",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRefreshSessions_UserId_RevokedAtUtc",
                schema: "identity",
                table: "AppRefreshSessions",
                columns: new[] { "UserId", "RevokedAtUtc" });
        }
    }
}
