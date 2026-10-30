using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomSewingVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItemVariants_Kind",
                schema: "crm",
                table: "QuoteItemVariants");

            migrationBuilder.AddColumn<long>(
                name: "CustomVersionId",
                schema: "crm",
                table: "QuoteItemVariants",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomSewingCalculations",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    BasisJson = table.Column<string>(type: "jsonb", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSewingCalculations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingDrafts",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    QuoteId = table.Column<long>(type: "bigint", nullable: true),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSewingDrafts", x => x.Id);
                    table.UniqueConstraint("AK_CustomSewingDrafts_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CustomDraft_Revision", "\"Revision\" > 0");
                    table.ForeignKey(
                        name: "FK_CustomSewingDrafts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "crm",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomSewingDrafts_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingMutationRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DraftPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSewingMutationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomSewingMutationRequests_CustomSewingDrafts_DraftPublic~",
                        column: x => x.DraftPublicId,
                        principalSchema: "crm",
                        principalTable: "CustomSewingDrafts",
                        principalColumn: "PublicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ParentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false),
                    Basis = table.Column<string>(type: "jsonb", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSewingVersions", x => x.Id);
                    table.UniqueConstraint("AK_CustomSewingVersions_DraftId_Id", x => new { x.DraftId, x.Id });
                    table.CheckConstraint("CK_CustomVersion_Number", "\"Number\" > 0");
                    table.ForeignKey(
                        name: "FK_CustomSewingVersions_CustomSewingDrafts_DraftId",
                        column: x => x.DraftId,
                        principalSchema: "crm",
                        principalTable: "CustomSewingDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomSewingVersions_CustomSewingVersions_DraftId_ParentVer~",
                        columns: x => new { x.DraftId, x.ParentVersionId },
                        principalSchema: "crm",
                        principalTable: "CustomSewingVersions",
                        principalColumns: new[] { "DraftId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItemVariants_CustomVersionId",
                schema: "crm",
                table: "QuoteItemVariants",
                column: "CustomVersionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItemVariants_Kind",
                schema: "crm",
                table: "QuoteItemVariants",
                sql: "\"Kind\" IN ('sewing','ppe','oneOffPpe','customSewing')");

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingCalculations_DraftId_Revision_ActorId",
                schema: "crm",
                table: "CustomSewingCalculations",
                columns: new[] { "DraftId", "Revision", "ActorId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingCalculations_Token",
                schema: "crm",
                table: "CustomSewingCalculations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingDrafts_CustomerId",
                schema: "crm",
                table: "CustomSewingDrafts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingDrafts_Id_CurrentVersionId",
                schema: "crm",
                table: "CustomSewingDrafts",
                columns: new[] { "Id", "CurrentVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingDrafts_OwnerId_UpdatedAt_Id",
                schema: "crm",
                table: "CustomSewingDrafts",
                columns: new[] { "OwnerId", "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingDrafts_QuoteId",
                schema: "crm",
                table: "CustomSewingDrafts",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingMutationRequests_ActorId_Operation_RequestId",
                schema: "crm",
                table: "CustomSewingMutationRequests",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingMutationRequests_DraftPublicId",
                schema: "crm",
                table: "CustomSewingMutationRequests",
                column: "DraftPublicId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingVersions_DraftId_Number",
                schema: "crm",
                table: "CustomSewingVersions",
                columns: new[] { "DraftId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingVersions_DraftId_ParentVersionId",
                schema: "crm",
                table: "CustomSewingVersions",
                columns: new[] { "DraftId", "ParentVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingVersions_PublicId",
                schema: "crm",
                table: "CustomSewingVersions",
                column: "PublicId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteItemVariants_CustomSewingVersions_CustomVersionId",
                schema: "crm",
                table: "QuoteItemVariants",
                column: "CustomVersionId",
                principalSchema: "crm",
                principalTable: "CustomSewingVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomSewingCalculations_CustomSewingDrafts_DraftId",
                schema: "crm",
                table: "CustomSewingCalculations",
                column: "DraftId",
                principalSchema: "crm",
                principalTable: "CustomSewingDrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomSewingDrafts_CustomSewingVersions_Id_CurrentVersionId",
                schema: "crm",
                table: "CustomSewingDrafts",
                columns: new[] { "Id", "CurrentVersionId" },
                principalSchema: "crm",
                principalTable: "CustomSewingVersions",
                principalColumns: new[] { "DraftId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuoteItemVariants_CustomSewingVersions_CustomVersionId",
                schema: "crm",
                table: "QuoteItemVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomSewingVersions_CustomSewingDrafts_DraftId",
                schema: "crm",
                table: "CustomSewingVersions");

            migrationBuilder.DropTable(
                name: "CustomSewingCalculations",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingMutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingDrafts",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingVersions",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_QuoteItemVariants_CustomVersionId",
                schema: "crm",
                table: "QuoteItemVariants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItemVariants_Kind",
                schema: "crm",
                table: "QuoteItemVariants");

            migrationBuilder.DropColumn(
                name: "CustomVersionId",
                schema: "crm",
                table: "QuoteItemVariants");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItemVariants_Kind",
                schema: "crm",
                table: "QuoteItemVariants",
                sql: "\"Kind\" IN ('sewing','ppe','oneOffPpe')");
        }
    }
}
