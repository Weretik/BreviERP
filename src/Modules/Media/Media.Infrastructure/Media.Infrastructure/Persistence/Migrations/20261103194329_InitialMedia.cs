using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Media.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "media");

            migrationBuilder.CreateTable(
                name: "Folders",
                schema: "media",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "media",
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                schema: "media",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Alt = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FolderId = table.Column<int>(type: "integer", nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PublicUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Folders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "media",
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId",
                schema: "media",
                table: "Files",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_StorageKey",
                schema: "media",
                table: "Files",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId",
                schema: "media",
                table: "Folders",
                column: "ParentId");

            // Moves the existing library from Catalog with the same ids, so product photos keep pointing at their files.
            // Only uploaded files are copied; unfinished uploads have no CDN file. They go into the folder «Товари».
            migrationBuilder.Sql(CopyCatalogMediaFiles);
        }

        private const string CopyCatalogMediaFiles = """
            DO $$
            DECLARE
                products_folder integer;
                last_id integer;
            BEGIN
                -- Nested IFs: PostgreSQL resolves a table only when the statement runs, so a new database without Catalog works.
                IF to_regclass('catalog."MediaFiles"') IS NOT NULL THEN
                    IF EXISTS (SELECT 1 FROM catalog."MediaFiles" WHERE "Status" = 2 AND "PublicUrl" IS NOT NULL) THEN
                        INSERT INTO media."Folders" ("Name", "ParentId", "CreatedAt", "UpdatedAt")
                        VALUES ('Товари', NULL, now(), now())
                        RETURNING "Id" INTO products_folder;

                        INSERT INTO media."Files" (
                            "Id", "FileName", "Alt", "FolderId", "ContentType", "SizeInBytes", "StorageProvider",
                            "StorageKey", "PublicUrl", "Width", "Height", "CreatedAt", "UpdatedAt")
                        OVERRIDING SYSTEM VALUE
                        SELECT "Id", "FileName", NULL, products_folder, "ContentType", "SizeInBytes", "StorageProvider",
                               "StorageKey", "PublicUrl", "Width", "Height", now(), now()
                        FROM catalog."MediaFiles"
                        WHERE "Status" = 2 AND "PublicUrl" IS NOT NULL;
                    END IF;
                END IF;

                SELECT MAX("Id") INTO last_id FROM media."Files";
                IF last_id IS NOT NULL THEN
                    PERFORM setval(pg_get_serial_sequence('media."Files"', 'Id'), last_id, true);
                END IF;
            END $$;
            """;

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files",
                schema: "media");

            migrationBuilder.DropTable(
                name: "Folders",
                schema: "media");
        }
    }
}
