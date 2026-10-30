using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTemplateRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailTemplateMutations",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    VerifiedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateVersions", x => x.Id);
                    table.CheckConstraint("CK_EmailTemplateVersions_Number", "\"Number\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateState",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    ActiveVersionId = table.Column<long>(type: "bigint", nullable: true),
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
            migrationBuilder.Sql("""
                CREATE FUNCTION crm.reject_email_template_version_mutation() RETURNS trigger
                LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'email_template_versions_immutable'; END; $$;
                CREATE TRIGGER email_template_versions_immutable
                BEFORE UPDATE OR DELETE ON crm."EmailTemplateVersions"
                FOR EACH ROW EXECUTE FUNCTION crm.reject_email_template_version_mutation();
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailTemplateMutations",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "EmailTemplateState",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "EmailTemplateVersions",
                schema: "crm");
            migrationBuilder.Sql("DROP FUNCTION crm.reject_email_template_version_mutation();");
        }
    }
}
