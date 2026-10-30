using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIssuerSettingsAndNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentNumberSequences",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    NextNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNumberSequences", x => x.Id);
                    table.CheckConstraint("CK_DocumentNumberSequence_Month", "\"Month\" BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_DocumentNumberSequence_Next", "\"NextNumber\" >= 10");
                });

            migrationBuilder.CreateTable(
                name: "DocumentSettingsMutations",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSettingsMutations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTemplateVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    LayoutKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VatNotice = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OpeningText = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CtaTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CtaText = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CtaContactTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    VerifiedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplateVersions", x => x.Id);
                    table.CheckConstraint("CK_DocumentTemplateVersion_Number", "\"Number\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "IssuerSettingsVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Edrpou = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Iban = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Site = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Mfo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LogoAssetKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VerifiedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuerSettingsVersions", x => x.Id);
                    table.CheckConstraint("CK_IssuerSettingsVersion_Number", "\"Number\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "DocumentSettingsState",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    ActiveIssuerVersionId = table.Column<long>(type: "bigint", nullable: true),
                    ActiveTemplateVersionId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSettingsState", x => x.Id);
                    table.CheckConstraint("CK_DocumentSettingsState_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_DocumentSettingsState_Singleton", "\"Id\" = 1");
                    table.ForeignKey(
                        name: "FK_DocumentSettingsState_DocumentTemplateVersions_ActiveTempla~",
                        column: x => x.ActiveTemplateVersionId,
                        principalSchema: "crm",
                        principalTable: "DocumentTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentSettingsState_IssuerSettingsVersions_ActiveIssuerVe~",
                        column: x => x.ActiveIssuerVersionId,
                        principalSchema: "crm",
                        principalTable: "IssuerSettingsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNumberSequences_Year_Month",
                schema: "crm",
                table: "DocumentNumberSequences",
                columns: new[] { "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSettingsMutations_ActorId_Operation_RequestId",
                schema: "crm",
                table: "DocumentSettingsMutations",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSettingsState_ActiveIssuerVersionId",
                schema: "crm",
                table: "DocumentSettingsState",
                column: "ActiveIssuerVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSettingsState_ActiveTemplateVersionId",
                schema: "crm",
                table: "DocumentSettingsState",
                column: "ActiveTemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplateVersions_Number",
                schema: "crm",
                table: "DocumentTemplateVersions",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplateVersions_PublicId",
                schema: "crm",
                table: "DocumentTemplateVersions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssuerSettingsVersions_Number",
                schema: "crm",
                table: "IssuerSettingsVersions",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssuerSettingsVersions_PublicId",
                schema: "crm",
                table: "IssuerSettingsVersions",
                column: "PublicId",
                unique: true);

            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_document_settings_history_change() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'document_settings_history_is_append_only' USING ERRCODE = '55000';
                END;
                $$;
                CREATE TRIGGER issuer_settings_version_append_only BEFORE UPDATE OR DELETE
                ON crm."IssuerSettingsVersions" FOR EACH ROW EXECUTE FUNCTION crm.reject_document_settings_history_change();
                CREATE TRIGGER document_template_version_append_only BEFORE UPDATE OR DELETE
                ON crm."DocumentTemplateVersions" FOR EACH ROW EXECUTE FUNCTION crm.reject_document_settings_history_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER document_template_version_append_only ON crm."DocumentTemplateVersions";
                DROP TRIGGER issuer_settings_version_append_only ON crm."IssuerSettingsVersions";
                DROP FUNCTION crm.reject_document_settings_history_change();
                """);
            migrationBuilder.DropTable(
                name: "DocumentNumberSequences",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "DocumentSettingsMutations",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "DocumentSettingsState",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "DocumentTemplateVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IssuerSettingsVersions",
                schema: "crm");
        }
    }
}
