using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIntroductionTemplatesAndFeaturedMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntroductionFeaturedMappingVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSnapshotHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SummerModelCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    WinterModelCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SummerProductId = table.Column<int>(type: "integer", nullable: false),
                    WinterProductId = table.Column<int>(type: "integer", nullable: false),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntroductionTemplates", x => x.Id);
                    table.UniqueConstraint("AK_IntroductionTemplates_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_IntroductionTemplate_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_IntroductionTemplate_Version", "\"VersionNumber\" >= 0");
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
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplatePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_introduction_history_change() RETURNS trigger
                LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'Introduction history is append-only';
                END;
                $$;
                CREATE TRIGGER introduction_template_version_append_only
                    BEFORE UPDATE OR DELETE ON crm."IntroductionTemplateVersions"
                    FOR EACH ROW EXECUTE FUNCTION crm.reject_introduction_history_change();
                CREATE TRIGGER introduction_featured_mapping_version_append_only
                    BEFORE UPDATE OR DELETE ON crm."IntroductionFeaturedMappingVersions"
                    FOR EACH ROW EXECUTE FUNCTION crm.reject_introduction_history_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER introduction_template_version_append_only ON crm."IntroductionTemplateVersions";
                DROP TRIGGER introduction_featured_mapping_version_append_only ON crm."IntroductionFeaturedMappingVersions";
                DROP FUNCTION crm.reject_introduction_history_change();
                """);
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
                name: "IntroductionFeaturedMappingVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "IntroductionTemplates",
                schema: "crm");
        }
    }
}
