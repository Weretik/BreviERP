using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomPrivateFileLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomPrivateFiles",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    State = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomPrivateFiles", x => x.Id);
                    table.UniqueConstraint("AK_CustomPrivateFiles_DraftId_Id", x => new { x.DraftId, x.Id });
                    table.UniqueConstraint("AK_CustomPrivateFiles_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CustomPrivateFile_Expiry", "\"ExpiresAt\" > \"CreatedAt\"");
                    table.CheckConstraint("CK_CustomPrivateFile_Length", "\"Length\" > 0 AND \"Length\" <= 20971520");
                    table.ForeignKey(
                        name: "FK_CustomPrivateFiles_CustomSewingDrafts_DraftId",
                        column: x => x.DraftId,
                        principalSchema: "crm",
                        principalTable: "CustomSewingDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomDraftAttachments",
                schema: "crm",
                columns: table => new
                {
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    ShowInQuote = table.Column<bool>(type: "boolean", nullable: false),
                    Production = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomDraftAttachments", x => new { x.DraftId, x.FileId });
                    table.ForeignKey(
                        name: "FK_CustomDraftAttachments_CustomPrivateFiles_DraftId_FileId",
                        columns: x => new { x.DraftId, x.FileId },
                        principalSchema: "crm",
                        principalTable: "CustomPrivateFiles",
                        principalColumns: new[] { "DraftId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomVersionAttachments",
                schema: "crm",
                columns: table => new
                {
                    VersionId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    ShowInQuote = table.Column<bool>(type: "boolean", nullable: false),
                    Production = table.Column<bool>(type: "boolean", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomVersionAttachments", x => new { x.VersionId, x.FileId });
                    table.ForeignKey(
                        name: "FK_CustomVersionAttachments_CustomPrivateFiles_DraftId_FileId",
                        columns: x => new { x.DraftId, x.FileId },
                        principalSchema: "crm",
                        principalTable: "CustomPrivateFiles",
                        principalColumns: new[] { "DraftId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomVersionAttachments_CustomSewingVersions_DraftId_Versi~",
                        columns: x => new { x.DraftId, x.VersionId },
                        principalSchema: "crm",
                        principalTable: "CustomSewingVersions",
                        principalColumns: new[] { "DraftId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomPrivateFiles_ActorId_RequestId",
                schema: "crm",
                table: "CustomPrivateFiles",
                columns: new[] { "ActorId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomPrivateFiles_DraftId_State",
                schema: "crm",
                table: "CustomPrivateFiles",
                columns: new[] { "DraftId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomPrivateFiles_State_ExpiresAt",
                schema: "crm",
                table: "CustomPrivateFiles",
                columns: new[] { "State", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomVersionAttachments_DraftId_FileId",
                schema: "crm",
                table: "CustomVersionAttachments",
                columns: new[] { "DraftId", "FileId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomVersionAttachments_DraftId_VersionId",
                schema: "crm",
                table: "CustomVersionAttachments",
                columns: new[] { "DraftId", "VersionId" });

            migrationBuilder.Sql("""
                CREATE TRIGGER custom_version_attachment_append_only BEFORE UPDATE OR DELETE
                ON crm."CustomVersionAttachments" FOR EACH ROW
                EXECUTE FUNCTION crm.reject_custom_history_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER custom_version_attachment_append_only ON crm."CustomVersionAttachments";
                """);
            migrationBuilder.DropTable(
                name: "CustomDraftAttachments",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomVersionAttachments",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomPrivateFiles",
                schema: "crm");
        }
    }
}
