using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuoteIssues",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    IssuerVersionId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteIssues", x => x.Id);
                    table.CheckConstraint("CK_QuoteIssues_Number", "length(\"Number\") > 0");
                    table.CheckConstraint("CK_QuoteIssues_QuoteVersionId", "\"QuoteVersionId\" > 0");
                    table.ForeignKey(
                        name: "FK_QuoteIssues_DocumentTemplateVersions_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalSchema: "crm",
                        principalTable: "DocumentTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteIssues_IssuerSettingsVersions_IssuerVersionId",
                        column: x => x.IssuerVersionId,
                        principalSchema: "crm",
                        principalTable: "IssuerSettingsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteIssues_QuoteVersions_QuoteId_QuoteVersionId",
                        columns: x => new { x.QuoteId, x.QuoteVersionId },
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteIssues_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotePdfArtifacts",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    IssuerVersionId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateVersionId = table.Column<long>(type: "bigint", nullable: false),
                    RenderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PdfBytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Length = table.Column<long>(type: "bigint", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotePdfArtifacts", x => x.Id);
                    table.CheckConstraint("CK_QuotePdfArtifacts_Bytes", "(\"PdfBytes\" IS NULL OR octet_length(\"PdfBytes\") <= 20971520)");
                    table.CheckConstraint("CK_QuotePdfArtifacts_ContentState", "(\"Status\" = 'succeeded' AND \"PdfBytes\" IS NOT NULL AND \"Sha256\" IS NOT NULL AND \"Length\" > 0 AND \"GeneratedAt\" IS NOT NULL) OR (\"Status\" <> 'succeeded' AND \"PdfBytes\" IS NULL)");
                    table.CheckConstraint("CK_QuotePdfArtifacts_Status", "\"Status\" IN ('queued','processing','succeeded','failed')");
                    table.ForeignKey(
                        name: "FK_QuotePdfArtifacts_DocumentTemplateVersions_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalSchema: "crm",
                        principalTable: "DocumentTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotePdfArtifacts_IssuerSettingsVersions_IssuerVersionId",
                        column: x => x.IssuerVersionId,
                        principalSchema: "crm",
                        principalTable: "IssuerSettingsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotePdfArtifacts_QuoteVersions_QuoteId_QuoteVersionId",
                        columns: x => new { x.QuoteId, x.QuoteVersionId },
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotePdfArtifacts_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIssues_IssuerVersionId",
                schema: "crm",
                table: "QuoteIssues",
                column: "IssuerVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIssues_QuoteId_QuoteVersionId",
                schema: "crm",
                table: "QuoteIssues",
                columns: new[] { "QuoteId", "QuoteVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIssues_TemplateVersionId",
                schema: "crm",
                table: "QuoteIssues",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotePdfArtifacts_IssuerVersionId",
                schema: "crm",
                table: "QuotePdfArtifacts",
                column: "IssuerVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotePdfArtifacts_PublicId",
                schema: "crm",
                table: "QuotePdfArtifacts",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotePdfArtifacts_QuoteId_QuoteVersionId_IssuerVersionId_Te~",
                schema: "crm",
                table: "QuotePdfArtifacts",
                columns: new[] { "QuoteId", "QuoteVersionId", "IssuerVersionId", "TemplateVersionId", "RenderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotePdfArtifacts_Status_LeaseUntil_CreatedAt",
                schema: "crm",
                table: "QuotePdfArtifacts",
                columns: new[] { "Status", "LeaseUntil", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuotePdfArtifacts_TemplateVersionId",
                schema: "crm",
                table: "QuotePdfArtifacts",
                column: "TemplateVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteIssues",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuotePdfArtifacts",
                schema: "crm");
        }
    }
}
