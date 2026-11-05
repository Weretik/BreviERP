using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Purchasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckedDocumentHistory",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Origin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CheckedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReplacedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReplacedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckedDocumentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckedDocumentHistory_MessageFiles_FileId",
                        column: x => x.FileId,
                        principalSchema: "purchasing",
                        principalTable: "MessageFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckedDocuments",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceUid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginalUrl = table.Column<string>(type: "text", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Origin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Model = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CostUsd = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: true),
                    CheckedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckedDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckedDocuments_MessageFiles_Id",
                        column: x => x.Id,
                        principalSchema: "purchasing",
                        principalTable: "MessageFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckedDocuments_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "purchasing",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecognitionAttempts",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Problems = table.Column<string[]>(type: "text[]", nullable: false),
                    Model = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CostUsd = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false),
                    OutputJson = table.Column<string>(type: "text", nullable: true),
                    By = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognitionAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecognitionAttempts_MessageFiles_FileId",
                        column: x => x.FileId,
                        principalSchema: "purchasing",
                        principalTable: "MessageFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecognitionAttempts_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "purchasing",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckedDocumentHistory_FileId_Id",
                schema: "purchasing",
                table: "CheckedDocumentHistory",
                columns: new[] { "FileId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckedDocuments_MessageId",
                schema: "purchasing",
                table: "CheckedDocuments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckedDocuments_SourceUid",
                schema: "purchasing",
                table: "CheckedDocuments",
                column: "SourceUid");

            migrationBuilder.CreateIndex(
                name: "IX_RecognitionAttempts_FileId_Id",
                schema: "purchasing",
                table: "RecognitionAttempts",
                columns: new[] { "FileId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RecognitionAttempts_MessageId",
                schema: "purchasing",
                table: "RecognitionAttempts",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckedDocumentHistory",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "CheckedDocuments",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "RecognitionAttempts",
                schema: "purchasing");
        }
    }
}
