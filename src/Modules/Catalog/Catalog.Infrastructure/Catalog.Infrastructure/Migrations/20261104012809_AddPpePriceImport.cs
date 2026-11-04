using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPpePriceImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PpePriceImportRuns",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Trigger = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Actor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Checked = table.Column<int>(type: "integer", nullable: false),
                    Changed = table.Column<int>(type: "integer", nullable: false),
                    Unchanged = table.Column<int>(type: "integer", nullable: false),
                    Skipped = table.Column<int>(type: "integer", nullable: false),
                    ProblemsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpePriceImportRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PpePriceSources",
                schema: "catalog",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpePriceSources", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PpePriceImportRuns_SourceKey_Id",
                schema: "catalog",
                table: "PpePriceImportRuns",
                columns: new[] { "SourceKey", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpePriceImportRuns",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "PpePriceSources",
                schema: "catalog");
        }
    }
}
