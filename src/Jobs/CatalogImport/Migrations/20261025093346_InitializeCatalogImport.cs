using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogImport.Migrations
{
    /// <inheritdoc />
    public partial class InitializeCatalogImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "imports");

            migrationBuilder.CreateTable(
                name: "CatalogImportBatches",
                schema: "imports",
                columns: table => new
                {
                    SnapshotHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SpreadsheetId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceJson = table.Column<string>(type: "jsonb", nullable: false),
                    ReportJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogImportBatches", x => x.SnapshotHash);
                });

            migrationBuilder.CreateTable(
                name: "CatalogSourceMappings",
                schema: "imports",
                columns: table => new
                {
                    Dataset = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetId = table.Column<long>(type: "bigint", nullable: false),
                    SnapshotHash = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogSourceMappings", x => new { x.Dataset, x.SourceKey });
                    table.ForeignKey(
                        name: "FK_CatalogSourceMappings_CatalogImportBatches_SnapshotHash",
                        column: x => x.SnapshotHash,
                        principalSchema: "imports",
                        principalTable: "CatalogImportBatches",
                        principalColumn: "SnapshotHash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogSourceMappings_SnapshotHash",
                schema: "imports",
                table: "CatalogSourceMappings",
                column: "SnapshotHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogSourceMappings",
                schema: "imports");

            migrationBuilder.DropTable(
                name: "CatalogImportBatches",
                schema: "imports");
        }
    }
}
