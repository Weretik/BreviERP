using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Removes the Codex КП, custom sewing, documents, delivery and introduction tables (replaced by the new КП).
    /// Down recreates empty tables without their history triggers; the removed data is not restored.
    /// </summary>
    public partial class RemoveCodexQuoteTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomSewingVersions_CustomSewingDrafts_DraftId",
                schema: "crm",
                table: "CustomSewingVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteVersions_Quotes_QuoteId",
                schema: "crm",
                table: "QuoteVersions");

            migrationBuilder.DropTable(
                name: "CalculationBases",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomDraftAttachments",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingCalculations",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingMutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingTemplateMutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingTemplateVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomVersionAttachments",
                schema: "crm");

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
                name: "EmailTemplateMutations",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "EmailTemplateState",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IntroductionFeaturedMappingState",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IntroductionTemplateMutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IntroductionTemplateVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "ProductionPackageRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveryEvents",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveryRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveryResolutions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveryStates",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteLifecycleEvents",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteMutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteVariantTiers",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingTemplates",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomPrivateFiles",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IntroductionFeaturedMappingVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IntroductionTemplates",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "ProductionPackageArtifacts",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteDeliveries",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteItemVariants",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "EmailTemplateVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuotePdfArtifacts",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteVersionItems",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteIssues",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "DocumentTemplateVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IssuerSettingsVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingDrafts",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Quotes",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteVersions",
                schema: "crm");

            // Trigger functions of the removed Codex tables; the tables are gone, so nothing uses them.
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS crm.protect_quote_delivery_evidence();
                DROP FUNCTION IF EXISTS crm.reject_custom_history_change();
                DROP FUNCTION IF EXISTS crm.reject_custom_template_history_change();
                DROP FUNCTION IF EXISTS crm.reject_document_settings_history_change();
                DROP FUNCTION IF EXISTS crm.reject_email_template_version_mutation();
                DROP FUNCTION IF EXISTS crm.reject_introduction_history_change();
                DROP FUNCTION IF EXISTS crm.reject_production_package_history_change();
                DROP FUNCTION IF EXISTS crm.reject_quote_history_change();
                DROP FUNCTION IF EXISTS crm.reject_quote_issue_change();
                DROP FUNCTION IF EXISTS crm.reject_succeeded_quote_pdf_change();
                DROP FUNCTION IF EXISTS crm.validate_custom_version_parent();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculationBases",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotePublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationBases", x => x.Id);
                    table.CheckConstraint("CK_CalculationBases_Expiry", "\"ExpiresAt\" > \"CreatedAt\"");
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingTemplates",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSewingTemplates", x => x.Id);
                    table.UniqueConstraint("AK_CustomSewingTemplates_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CustomTemplate_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_CustomTemplate_Version", "\"VersionNumber\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "DocumentNumberSequences",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    NextNumber = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false)
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CtaContactTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CtaText = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CtaTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LayoutKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    OpeningText = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VatNotice = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    VerifiedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplateVersions", x => x.Id);
                    table.CheckConstraint("CK_DocumentTemplateVersion_Number", "\"Number\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateMutations",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateMutations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Body = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VerifiedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateVersions", x => x.Id);
                    table.CheckConstraint("CK_EmailTemplateVersions_Number", "\"Number\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "IntroductionFeaturedMappingVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSnapshotHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SummerModelCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SummerProductId = table.Column<int>(type: "integer", nullable: false),
                    WinterModelCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    WinterProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntroductionFeaturedMappingVersions", x => x.Id);
                    table.UniqueConstraint("AK_IntroductionFeaturedMappingVersions_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_IntroductionFeaturedMappingVersion_Products", "\"SummerProductId\" > 0 AND \"WinterProductId\" > 0 AND \"SummerProductId\" <> \"WinterProductId\"");
                });

            migrationBuilder.CreateTable(
                name: "IntroductionTemplates",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntroductionTemplates", x => x.Id);
                    table.UniqueConstraint("AK_IntroductionTemplates_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_IntroductionTemplate_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_IntroductionTemplate_Version", "\"VersionNumber\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "IssuerSettingsVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Edrpou = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Iban = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoAssetKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Mfo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Site = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VerifiedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuerSettingsVersions", x => x.Id);
                    table.CheckConstraint("CK_IssuerSettingsVersion_Number", "\"Number\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "QuoteMutationRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    QuotePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteMutationRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingTemplateMutationRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    TemplatePublicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSewingTemplateMutationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomSewingTemplateMutationRequests_CustomSewingTemplates_~",
                        column: x => x.TemplatePublicId,
                        principalSchema: "crm",
                        principalTable: "CustomSewingTemplates",
                        principalColumn: "PublicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingTemplateVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Introduction = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Selections = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSewingTemplateVersions", x => x.Id);
                    table.UniqueConstraint("AK_CustomSewingTemplateVersions_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CustomTemplateVersion_Number", "\"Number\" > 0");
                    table.ForeignKey(
                        name: "FK_CustomSewingTemplateVersions_CustomSewingTemplates_Template~",
                        column: x => x.TemplateId,
                        principalSchema: "crm",
                        principalTable: "CustomSewingTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateState",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ActiveVersionId = table.Column<long>(type: "bigint", nullable: true),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateState", x => x.Id);
                    table.CheckConstraint("CK_EmailTemplateState_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_EmailTemplateState_Singleton", "\"Id\" = 1");
                    table.ForeignKey(
                        name: "FK_EmailTemplateState_EmailTemplateVersions_ActiveVersionId",
                        column: x => x.ActiveVersionId,
                        principalSchema: "crm",
                        principalTable: "EmailTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntroductionFeaturedMappingState",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ActiveVersionId = table.Column<long>(type: "bigint", nullable: true),
                    Revision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntroductionFeaturedMappingState", x => x.Id);
                    table.CheckConstraint("CK_IntroductionFeaturedMappingState_Id", "\"Id\" = 1");
                    table.CheckConstraint("CK_IntroductionFeaturedMappingState_Revision", "\"Revision\" > 0");
                    table.ForeignKey(
                        name: "FK_IntroductionFeaturedMappingState_IntroductionFeaturedMappin~",
                        column: x => x.ActiveVersionId,
                        principalSchema: "crm",
                        principalTable: "IntroductionFeaturedMappingVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntroductionTemplateMutationRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    TemplatePublicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntroductionTemplateMutationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntroductionTemplateMutationRequests_IntroductionTemplates_~",
                        column: x => x.TemplatePublicId,
                        principalSchema: "crm",
                        principalTable: "IntroductionTemplates",
                        principalColumn: "PublicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntroductionTemplateVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntroductionTemplateVersions", x => x.Id);
                    table.UniqueConstraint("AK_IntroductionTemplateVersions_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_IntroductionTemplateVersion_Number", "\"Number\" > 0");
                    table.ForeignKey(
                        name: "FK_IntroductionTemplateVersions_IntroductionTemplates_Template~",
                        column: x => x.TemplateId,
                        principalSchema: "crm",
                        principalTable: "IntroductionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSettingsState",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ActiveIssuerVersionId = table.Column<long>(type: "bigint", nullable: true),
                    ActiveTemplateVersionId = table.Column<long>(type: "bigint", nullable: true),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "CustomDraftAttachments",
                schema: "crm",
                columns: table => new
                {
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    Production = table.Column<bool>(type: "boolean", nullable: false),
                    ShowInQuote = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomDraftAttachments", x => new { x.DraftId, x.FileId });
                });

            migrationBuilder.CreateTable(
                name: "CustomPrivateFiles",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    State = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomPrivateFiles", x => x.Id);
                    table.UniqueConstraint("AK_CustomPrivateFiles_DraftId_Id", x => new { x.DraftId, x.Id });
                    table.UniqueConstraint("AK_CustomPrivateFiles_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CustomPrivateFile_Expiry", "\"ExpiresAt\" > \"CreatedAt\"");
                    table.CheckConstraint("CK_CustomPrivateFile_Length", "\"Length\" > 0 AND \"Length\" <= 20971520");
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingCalculations",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    BasisJson = table.Column<string>(type: "jsonb", nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<Guid>(type: "uuid", nullable: false)
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
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    QuoteId = table.Column<long>(type: "bigint", nullable: true),
                    CurrentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    Season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TemplateOrigin = table.Column<string>(type: "jsonb", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingMutationRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DraftPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false)
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
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    ParentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Basis = table.Column<string>(type: "jsonb", nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "CustomVersionAttachments",
                schema: "crm",
                columns: table => new
                {
                    VersionId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    Production = table.Column<bool>(type: "boolean", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ShowInQuote = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ProductionPackageArtifacts",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bytes = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    SelectionHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPackageArtifacts", x => x.Id);
                    table.UniqueConstraint("AK_ProductionPackageArtifacts_QuoteId_Id", x => new { x.QuoteId, x.Id });
                    table.CheckConstraint("CK_ProductionPackageArtifacts_Content", "octet_length(\"Bytes\") = \"Length\" AND \"Length\" > 0 AND \"Length\" <= 67108864");
                    table.CheckConstraint("CK_ProductionPackageArtifacts_Schema", "\"SchemaVersion\" = 1");
                });

            migrationBuilder.CreateTable(
                name: "ProductionPackageRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PackageArtifactId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPackageRequests", x => x.Id);
                    table.CheckConstraint("CK_ProductionPackageRequests_Actor", "\"ActorId\" > 0");
                    table.ForeignKey(
                        name: "FK_ProductionPackageRequests_ProductionPackageArtifacts_QuoteI~",
                        columns: x => new { x.QuoteId, x.PackageArtifactId },
                        principalSchema: "crm",
                        principalTable: "ProductionPackageArtifacts",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveries",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    ArtifactId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EmailTemplateVersionId = table.Column<long>(type: "bigint", nullable: false),
                    PdfBytes = table.Column<byte[]>(type: "bytea", nullable: false),
                    PdfSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlainText = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    RecipientAddress = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    SenderAddress = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveries", x => x.Id);
                    table.UniqueConstraint("AK_QuoteDeliveries_QuoteId_Id", x => new { x.QuoteId, x.Id });
                    table.UniqueConstraint("AK_QuoteDeliveries_QuoteId_QuoteVersionId_Id", x => new { x.QuoteId, x.QuoteVersionId, x.Id });
                    table.CheckConstraint("CK_QuoteDeliveries_Bytes", "octet_length(\"PdfBytes\") BETWEEN 5 AND 20971520");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveries_EmailTemplateVersions_EmailTemplateVersionId",
                        column: x => x.EmailTemplateVersionId,
                        principalSchema: "crm",
                        principalTable: "EmailTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryEvents",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryEvents", x => x.Id);
                    table.CheckConstraint("CK_QuoteDeliveryEvents_Kind", "\"Kind\" IN ('queued','submit_started','reconcile_started','succeeded','failed','unknown','cancelled','manual_succeeded','manual_failed')");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryEvents_QuoteDeliveries_QuoteId_QuoteVersionId_~",
                        columns: x => new { x.QuoteId, x.QuoteVersionId, x.DeliveryId },
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumns: new[] { "QuoteId", "QuoteVersionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryRequests_QuoteDeliveries_QuoteId_DeliveryId",
                        columns: x => new { x.QuoteId, x.DeliveryId },
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryResolutions",
                schema: "crm",
                columns: table => new
                {
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EvidenceNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryResolutions", x => x.DeliveryId);
                    table.CheckConstraint("CK_QuoteDeliveryResolutions_ActorAttempt", "\"ActorId\" > 0 AND \"Attempt\" > 0");
                    table.CheckConstraint("CK_QuoteDeliveryResolutions_Evidence", "length(btrim(\"EvidenceNote\")) > 0");
                    table.CheckConstraint("CK_QuoteDeliveryResolutions_Outcome", "\"Outcome\" IN ('sent','not_sent')");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryResolutions_QuoteDeliveries_QuoteId_DeliveryId",
                        columns: x => new { x.QuoteId, x.DeliveryId },
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteDeliveryStates",
                schema: "crm",
                columns: table => new
                {
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextCheckAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteDeliveryStates", x => x.DeliveryId);
                    table.CheckConstraint("CK_QuoteDeliveryStates_Attempt", "\"Attempt\" >= 0");
                    table.CheckConstraint("CK_QuoteDeliveryStates_Lease", "(\"Status\" IN ('sending','reconciling') AND \"LeaseToken\" IS NOT NULL AND \"LeaseUntil\" IS NOT NULL) OR (\"Status\" NOT IN ('sending','reconciling') AND \"LeaseToken\" IS NULL AND \"LeaseUntil\" IS NULL)");
                    table.CheckConstraint("CK_QuoteDeliveryStates_Status", "\"Status\" IN ('queued','sending','reconciling','unknown','succeeded','failed')");
                    table.ForeignKey(
                        name: "FK_QuoteDeliveryStates_QuoteDeliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalSchema: "crm",
                        principalTable: "QuoteDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteIssues",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IssuerVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    ReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TemplateVersionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteIssues", x => x.Id);
                    table.UniqueConstraint("AK_QuoteIssues_QuoteId_QuoteVersionId_IssuerVersionId_Template~", x => new { x.QuoteId, x.QuoteVersionId, x.IssuerVersionId, x.TemplateVersionId });
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
                });

            migrationBuilder.CreateTable(
                name: "QuoteItemVariants",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    BasisToken = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomVersionId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PriceBasisJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Visible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItemVariants", x => x.Id);
                    table.CheckConstraint("CK_QuoteItemVariants_CustomVersion", "(\"Kind\" = 'customSewing') = (\"CustomVersionId\" IS NOT NULL)");
                    table.CheckConstraint("CK_QuoteItemVariants_Kind", "\"Kind\" IN ('sewing','ppe','oneOffPpe','customSewing')");
                    table.ForeignKey(
                        name: "FK_QuoteItemVariants_CustomSewingVersions_CustomVersionId",
                        column: x => x.CustomVersionId,
                        principalSchema: "crm",
                        principalTable: "CustomSewingVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteVariantTiers",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    VariantId = table.Column<long>(type: "bigint", nullable: false),
                    Visible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteVariantTiers", x => x.Id);
                    table.CheckConstraint("CK_QuoteVariantTiers_Amount", "\"Amount\" IS NULL OR \"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_QuoteVariantTiers_QuoteItemVariants_VariantId",
                        column: x => x.VariantId,
                        principalSchema: "crm",
                        principalTable: "QuoteItemVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteLifecycleEvents",
                schema: "crm",
                columns: table => new
                {
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    PreviousOwnerId = table.Column<int>(type: "integer", nullable: true),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteLifecycleEvents", x => x.PublicId);
                    table.CheckConstraint("CK_QuoteLifecycleEvents_Revision", "\"Revision\" > 0 AND \"VersionNumber\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "QuotePdfArtifacts",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssuerVersionId = table.Column<long>(type: "bigint", nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Length = table.Column<long>(type: "bigint", nullable: true),
                    PdfBytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    QuoteVersionId = table.Column<long>(type: "bigint", nullable: false),
                    RenderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TemplateVersionId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotePdfArtifacts", x => x.Id);
                    table.UniqueConstraint("AK_QuotePdfArtifacts_QuoteId_QuoteVersionId_Id", x => new { x.QuoteId, x.QuoteVersionId, x.Id });
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
                        name: "FK_QuotePdfArtifacts_QuoteIssues_QuoteId_QuoteVersionId_Issuer~",
                        columns: x => new { x.QuoteId, x.QuoteVersionId, x.IssuerVersionId, x.TemplateVersionId },
                        principalSchema: "crm",
                        principalTable: "QuoteIssues",
                        principalColumns: new[] { "QuoteId", "QuoteVersionId", "IssuerVersionId", "TemplateVersionId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    ContactPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    IssuedVersionId = table.Column<long>(type: "bigint", nullable: true),
                    Number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    SourceQuotePublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceVersionPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.CheckConstraint("CK_Quotes_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_Quotes_State", "\"State\" IN ('draft','ready','sent')");
                    table.ForeignKey(
                        name: "FK_Quotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "crm",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    ContactPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CustomerPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    IntroductionPublicSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    ParentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TaxBasis = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteVersions", x => x.Id);
                    table.UniqueConstraint("AK_QuoteVersions_QuoteId_Id", x => new { x.QuoteId, x.Id });
                    table.CheckConstraint("CK_QuoteVersions_Number", "\"VersionNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_QuoteVersions_QuoteVersions_QuoteId_ParentVersionId",
                        columns: x => new { x.QuoteId, x.ParentVersionId },
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumns: new[] { "QuoteId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteVersions_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteVersionItems",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Included = table.Column<bool>(type: "boolean", nullable: false),
                    InternalNote = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    PublicComment = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    PublicDescription = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    VersionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteVersionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteVersionItems_QuoteVersions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculationBases_ActorId_RequestId",
                schema: "crm",
                table: "CalculationBases",
                columns: new[] { "ActorId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculationBases_ExpiresAt",
                schema: "crm",
                table: "CalculationBases",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CalculationBases_PublicId",
                schema: "crm",
                table: "CalculationBases",
                column: "PublicId",
                unique: true);

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
                name: "IX_CustomSewingTemplateMutationRequests_ActorId_Operation_Requ~",
                schema: "crm",
                table: "CustomSewingTemplateMutationRequests",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingTemplateMutationRequests_TemplatePublicId",
                schema: "crm",
                table: "CustomSewingTemplateMutationRequests",
                column: "TemplatePublicId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingTemplates_OwnerId_Archived_UpdatedAt_Id",
                schema: "crm",
                table: "CustomSewingTemplates",
                columns: new[] { "OwnerId", "Archived", "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomSewingTemplateVersions_TemplateId_Number",
                schema: "crm",
                table: "CustomSewingTemplateVersions",
                columns: new[] { "TemplateId", "Number" },
                unique: true);

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
                name: "IX_EmailTemplateMutations_ActorId_Operation_RequestId",
                schema: "crm",
                table: "EmailTemplateMutations",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateState_ActiveVersionId",
                schema: "crm",
                table: "EmailTemplateState",
                column: "ActiveVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateVersions_Number",
                schema: "crm",
                table: "EmailTemplateVersions",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateVersions_PublicId",
                schema: "crm",
                table: "EmailTemplateVersions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntroductionFeaturedMappingState_ActiveVersionId",
                schema: "crm",
                table: "IntroductionFeaturedMappingState",
                column: "ActiveVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_IntroductionFeaturedMappingVersions_SourceSnapshotHash",
                schema: "crm",
                table: "IntroductionFeaturedMappingVersions",
                column: "SourceSnapshotHash");

            migrationBuilder.CreateIndex(
                name: "IX_IntroductionTemplateMutationRequests_ActorId_Operation_Requ~",
                schema: "crm",
                table: "IntroductionTemplateMutationRequests",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntroductionTemplateMutationRequests_TemplatePublicId",
                schema: "crm",
                table: "IntroductionTemplateMutationRequests",
                column: "TemplatePublicId");

            migrationBuilder.CreateIndex(
                name: "IX_IntroductionTemplates_OwnerId_Archived_UpdatedAt_Id",
                schema: "crm",
                table: "IntroductionTemplates",
                columns: new[] { "OwnerId", "Archived", "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_IntroductionTemplateVersions_TemplateId_Number",
                schema: "crm",
                table: "IntroductionTemplateVersions",
                columns: new[] { "TemplateId", "Number" },
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

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageArtifacts_PublicId",
                schema: "crm",
                table: "ProductionPackageArtifacts",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageArtifacts_QuoteId_QuoteVersionId_Selection~",
                schema: "crm",
                table: "ProductionPackageArtifacts",
                columns: new[] { "QuoteId", "QuoteVersionId", "SelectionHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageRequests_ActorId_RequestId",
                schema: "crm",
                table: "ProductionPackageRequests",
                columns: new[] { "ActorId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackageRequests_QuoteId_PackageArtifactId",
                schema: "crm",
                table: "ProductionPackageRequests",
                columns: new[] { "QuoteId", "PackageArtifactId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_EmailTemplateVersionId",
                schema: "crm",
                table: "QuoteDeliveries",
                column: "EmailTemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_PublicId",
                schema: "crm",
                table: "QuoteDeliveries",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_QuoteId_CreatedAt",
                schema: "crm",
                table: "QuoteDeliveries",
                columns: new[] { "QuoteId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveries_QuoteId_QuoteVersionId_ArtifactId",
                schema: "crm",
                table: "QuoteDeliveries",
                columns: new[] { "QuoteId", "QuoteVersionId", "ArtifactId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryEvents_DeliveryId_CreatedAt",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                columns: new[] { "DeliveryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryEvents_DeliveryId_Kind",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                columns: new[] { "DeliveryId", "Kind" },
                unique: true,
                filter: "\"Kind\" = 'succeeded'");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryEvents_QuoteId_QuoteVersionId_DeliveryId",
                schema: "crm",
                table: "QuoteDeliveryEvents",
                columns: new[] { "QuoteId", "QuoteVersionId", "DeliveryId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryRequests_ActorId_RequestId",
                schema: "crm",
                table: "QuoteDeliveryRequests",
                columns: new[] { "ActorId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryRequests_QuoteId_DeliveryId",
                schema: "crm",
                table: "QuoteDeliveryRequests",
                columns: new[] { "QuoteId", "DeliveryId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryResolutions_QuoteId_DeliveryId",
                schema: "crm",
                table: "QuoteDeliveryResolutions",
                columns: new[] { "QuoteId", "DeliveryId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteDeliveryStates_Status_LeaseUntil_NextCheckAt_UpdatedAt",
                schema: "crm",
                table: "QuoteDeliveryStates",
                columns: new[] { "Status", "LeaseUntil", "NextCheckAt", "UpdatedAt" });

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
                name: "IX_QuoteItemVariants_CustomVersionId",
                schema: "crm",
                table: "QuoteItemVariants",
                column: "CustomVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItemVariants_ItemId_SortOrder",
                schema: "crm",
                table: "QuoteItemVariants",
                columns: new[] { "ItemId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLifecycleEvents_QuoteId_Revision",
                schema: "crm",
                table: "QuoteLifecycleEvents",
                columns: new[] { "QuoteId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteMutationRequests_ActorId_Operation_RequestId",
                schema: "crm",
                table: "QuoteMutationRequests",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteMutationRequests_QuotePublicId",
                schema: "crm",
                table: "QuoteMutationRequests",
                column: "QuotePublicId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CreatedAt_Id",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CustomerId",
                schema: "crm",
                table: "Quotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Id_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "CurrentVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Id_IssuedVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "IssuedVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Number",
                schema: "crm",
                table: "Quotes",
                column: "Number",
                unique: true,
                filter: "\"Number\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_OwnerId_Archived_UpdatedAt_Id",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "OwnerId", "Archived", "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_PublicId",
                schema: "crm",
                table: "Quotes",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_SourceQuotePublicId_Id",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "SourceQuotePublicId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVariantTiers_VariantId_Code",
                schema: "crm",
                table: "QuoteVariantTiers",
                columns: new[] { "VariantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersionItems_VersionId_SortOrder",
                schema: "crm",
                table: "QuoteVersionItems",
                columns: new[] { "VersionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_PublicId",
                schema: "crm",
                table: "QuoteVersions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_QuoteId_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions",
                columns: new[] { "QuoteId", "ParentVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_QuoteId_VersionNumber",
                schema: "crm",
                table: "QuoteVersions",
                columns: new[] { "QuoteId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomDraftAttachments_CustomPrivateFiles_DraftId_FileId",
                schema: "crm",
                table: "CustomDraftAttachments",
                columns: new[] { "DraftId", "FileId" },
                principalSchema: "crm",
                principalTable: "CustomPrivateFiles",
                principalColumns: new[] { "DraftId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPrivateFiles_CustomSewingDrafts_DraftId",
                schema: "crm",
                table: "CustomPrivateFiles",
                column: "DraftId",
                principalSchema: "crm",
                principalTable: "CustomSewingDrafts",
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

            migrationBuilder.AddForeignKey(
                name: "FK_CustomSewingDrafts_Quotes_QuoteId",
                schema: "crm",
                table: "CustomSewingDrafts",
                column: "QuoteId",
                principalSchema: "crm",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackageArtifacts_QuoteVersions_QuoteId_QuoteVersi~",
                schema: "crm",
                table: "ProductionPackageArtifacts",
                columns: new[] { "QuoteId", "QuoteVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackageArtifacts_Quotes_QuoteId",
                schema: "crm",
                table: "ProductionPackageArtifacts",
                column: "QuoteId",
                principalSchema: "crm",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackageRequests_Quotes_QuoteId",
                schema: "crm",
                table: "ProductionPackageRequests",
                column: "QuoteId",
                principalSchema: "crm",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteDeliveries_QuotePdfArtifacts_QuoteId_QuoteVersionId_Ar~",
                schema: "crm",
                table: "QuoteDeliveries",
                columns: new[] { "QuoteId", "QuoteVersionId", "ArtifactId" },
                principalSchema: "crm",
                principalTable: "QuotePdfArtifacts",
                principalColumns: new[] { "QuoteId", "QuoteVersionId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteDeliveries_QuoteVersions_QuoteId_QuoteVersionId",
                schema: "crm",
                table: "QuoteDeliveries",
                columns: new[] { "QuoteId", "QuoteVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteDeliveries_Quotes_QuoteId",
                schema: "crm",
                table: "QuoteDeliveries",
                column: "QuoteId",
                principalSchema: "crm",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteIssues_QuoteVersions_QuoteId_QuoteVersionId",
                schema: "crm",
                table: "QuoteIssues",
                columns: new[] { "QuoteId", "QuoteVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteIssues_Quotes_QuoteId",
                schema: "crm",
                table: "QuoteIssues",
                column: "QuoteId",
                principalSchema: "crm",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteItemVariants_QuoteVersionItems_ItemId",
                schema: "crm",
                table: "QuoteItemVariants",
                column: "ItemId",
                principalSchema: "crm",
                principalTable: "QuoteVersionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLifecycleEvents_Quotes_QuoteId",
                schema: "crm",
                table: "QuoteLifecycleEvents",
                column: "QuoteId",
                principalSchema: "crm",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotePdfArtifacts_QuoteVersions_QuoteId_QuoteVersionId",
                schema: "crm",
                table: "QuotePdfArtifacts",
                columns: new[] { "QuoteId", "QuoteVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotePdfArtifacts_Quotes_QuoteId",
                schema: "crm",
                table: "QuotePdfArtifacts",
                column: "QuoteId",
                principalSchema: "crm",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_QuoteVersions_Id_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "CurrentVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_QuoteVersions_Id_IssuedVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "IssuedVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
