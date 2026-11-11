using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseLedgerRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Destination",
                schema: "warehouse",
                table: "FabricMovements",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                schema: "warehouse",
                table: "FabricMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockOperations",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ColorKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Color = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    MetersAfter = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockOperations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_OperationId",
                schema: "warehouse",
                table: "FabricMovements",
                column: "OperationId");

            // The chain of each line now runs by operation date, record time and id (TZ v133 §6.3): restate
            // before/after of the existing movements in that order. Balances do not change: the sum of changes stays.
            migrationBuilder.Sql("""
                UPDATE warehouse."FabricMovements" AS m
                SET "MetersBefore" = r.total - m."Delta", "MetersAfter" = r.total
                FROM (
                    SELECT "Id", SUM("Delta") OVER (PARTITION BY "StockId"
                        ORDER BY "OperationDate", "CreatedAt", "Id" ROWS UNBOUNDED PRECEDING) AS total
                    FROM warehouse."FabricMovements"
                ) AS r
                WHERE r."Id" = m."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockOperations",
                schema: "warehouse");

            migrationBuilder.DropIndex(
                name: "IX_FabricMovements_OperationId",
                schema: "warehouse",
                table: "FabricMovements");

            migrationBuilder.DropColumn(
                name: "Destination",
                schema: "warehouse",
                table: "FabricMovements");

            migrationBuilder.DropColumn(
                name: "OperationId",
                schema: "warehouse",
                table: "FabricMovements");
        }
    }
}
