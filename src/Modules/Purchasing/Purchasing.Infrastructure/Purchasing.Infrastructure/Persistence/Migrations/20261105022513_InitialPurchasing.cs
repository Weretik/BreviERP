using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Purchasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPurchasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "purchasing");

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TelegramKey = table.Column<string>(type: "text", nullable: false),
                    MessageAt = table.Column<string>(type: "text", nullable: true),
                    MessageDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: false),
                    StatusOriginal = table.Column<string>(type: "text", nullable: false),
                    MarkAt = table.Column<string>(type: "text", nullable: true),
                    MarkBy = table.Column<string>(type: "text", nullable: true),
                    PaymentState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewVersion = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncStates",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordCount = table.Column<int>(type: "integer", nullable: false),
                    FileCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageFiles",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    SeenRun = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Issue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageFiles_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "purchasing",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SourceRows",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceUid = table.Column<string>(type: "text", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    RawJson = table.Column<string>(type: "text", nullable: false),
                    Issues = table.Column<List<string>>(type: "text[]", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    SeenRun = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceRows_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "purchasing",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SourceRevisions",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RawJson = table.Column<string>(type: "text", nullable: false),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceRevisions_SourceRows_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "purchasing",
                        principalTable: "SourceRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageFiles_MessageId_Url",
                schema: "purchasing",
                table: "MessageFiles",
                columns: new[] { "MessageId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MessageDate",
                schema: "purchasing",
                table: "Messages",
                column: "MessageDate");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SupplierId",
                schema: "purchasing",
                table: "Messages",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TelegramKey",
                schema: "purchasing",
                table: "Messages",
                column: "TelegramKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceRevisions_SourceId",
                schema: "purchasing",
                table: "SourceRevisions",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceRows_MessageId",
                schema: "purchasing",
                table: "SourceRows",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceRows_SourceUid",
                schema: "purchasing",
                table: "SourceRows",
                column: "SourceUid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageFiles",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "SourceRevisions",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "SyncStates",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "SourceRows",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "purchasing");
        }
    }
}
