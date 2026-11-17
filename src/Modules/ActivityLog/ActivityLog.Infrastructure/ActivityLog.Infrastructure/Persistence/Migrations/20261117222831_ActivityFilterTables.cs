using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivityLog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActivityFilterTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityActors",
                schema: "activity_log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityActors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivitySubjectTypes",
                schema: "activity_log",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySubjectTypes", x => x.Key);
                });

            // The filters start with every person and object type already in the journal, by their latest entry.
            migrationBuilder.Sql("""
                INSERT INTO activity_log."ActivityActors" ("Id", "Name", "LastSeenAt")
                SELECT DISTINCT ON ("ActorId") "ActorId", "ActorName", "OccurredAt"
                FROM activity_log."ActivityEntries"
                ORDER BY "ActorId", "OccurredAt" DESC
                ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO activity_log."ActivitySubjectTypes" ("Key", "Title", "LastSeenAt")
                SELECT DISTINCT ON ("SubjectType") "SubjectType", "SubjectTypeTitle", "OccurredAt"
                FROM activity_log."ActivityEntries"
                ORDER BY "SubjectType", "OccurredAt" DESC
                ON CONFLICT ("Key") DO NOTHING;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityEntries_ActorRoles",
                schema: "activity_log",
                table: "ActivityEntries",
                column: "ActorRoles")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityActors",
                schema: "activity_log");

            migrationBuilder.DropTable(
                name: "ActivitySubjectTypes",
                schema: "activity_log");

            migrationBuilder.DropIndex(
                name: "IX_ActivityEntries_ActorRoles",
                schema: "activity_log",
                table: "ActivityEntries");
        }
    }
}
