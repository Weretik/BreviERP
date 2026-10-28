using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomSewingTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateOrigin",
                schema: "crm",
                table: "CustomSewingDrafts",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomSewingTemplates",
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
                    table.PrimaryKey("PK_CustomSewingTemplates", x => x.Id);
                    table.UniqueConstraint("AK_CustomSewingTemplates_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CustomTemplate_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_CustomTemplate_Version", "\"VersionNumber\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "CustomSewingTemplateMutationRequests",
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
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Introduction = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Selections = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_custom_template_history_change() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'custom_template_history_is_append_only';
                END;
                $$;
                CREATE TRIGGER custom_template_version_append_only BEFORE UPDATE OR DELETE
                ON crm."CustomSewingTemplateVersions" FOR EACH ROW
                EXECUTE FUNCTION crm.reject_custom_template_history_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER custom_template_version_append_only ON crm."CustomSewingTemplateVersions";
                DROP FUNCTION crm.reject_custom_template_history_change();
                """);
            migrationBuilder.DropTable(
                name: "CustomSewingTemplateMutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingTemplateVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomSewingTemplates",
                schema: "crm");

            migrationBuilder.DropColumn(
                name: "TemplateOrigin",
                schema: "crm",
                table: "CustomSewingDrafts");
        }
    }
}
