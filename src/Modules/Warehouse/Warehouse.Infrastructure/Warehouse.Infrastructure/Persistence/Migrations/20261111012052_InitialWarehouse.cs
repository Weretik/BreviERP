using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "warehouse");

            migrationBuilder.CreateTable(
                name: "FabricIssues",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false),
                    ModelName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReversedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReversedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReverseReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricStocks",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FabricId = table.Column<int>(type: "integer", nullable: true),
                    FabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FabricName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Color = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ColorKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Meters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    HasMovements = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricStocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricMovements",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MetersBefore = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Delta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MetersAfter = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    OperationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Document = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    ItemId = table.Column<int>(type: "integer", nullable: true),
                    OrderNumber = table.Column<int>(type: "integer", nullable: true),
                    ModelName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ReversesMovementId = table.Column<int>(type: "integer", nullable: true),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricMovements_FabricMovements_ReversesMovementId",
                        column: x => x.ReversesMovementId,
                        principalSchema: "warehouse",
                        principalTable: "FabricMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricMovements_FabricStocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "warehouse",
                        principalTable: "FabricStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FabricIssueLines",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IssueId = table.Column<int>(type: "integer", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlanFabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PlanColorKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PlannedMeters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    StockId = table.Column<int>(type: "integer", nullable: false),
                    FabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FabricName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Color = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Meters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CoveredMeters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MovementId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricIssueLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricIssueLines_FabricIssues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "warehouse",
                        principalTable: "FabricIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FabricIssueLines_FabricMovements_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "warehouse",
                        principalTable: "FabricMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricIssueLines_FabricStocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "warehouse",
                        principalTable: "FabricStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssueLines_IssueId",
                schema: "warehouse",
                table: "FabricIssueLines",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssueLines_MovementId",
                schema: "warehouse",
                table: "FabricIssueLines",
                column: "MovementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssueLines_StockId",
                schema: "warehouse",
                table: "FabricIssueLines",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssues_ItemId",
                schema: "warehouse",
                table: "FabricIssues",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssues_LegacyId",
                schema: "warehouse",
                table: "FabricIssues",
                column: "LegacyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_ItemId",
                schema: "warehouse",
                table: "FabricMovements",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_LegacyId",
                schema: "warehouse",
                table: "FabricMovements",
                column: "LegacyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_OperationDate",
                schema: "warehouse",
                table: "FabricMovements",
                column: "OperationDate");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_ReversesMovementId",
                schema: "warehouse",
                table: "FabricMovements",
                column: "ReversesMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_StockId_Id",
                schema: "warehouse",
                table: "FabricMovements",
                columns: new[] { "StockId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_FabricStocks_FabricCode_ColorKey",
                schema: "warehouse",
                table: "FabricStocks",
                columns: new[] { "FabricCode", "ColorKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricStocks_LegacyId",
                schema: "warehouse",
                table: "FabricStocks",
                column: "LegacyId",
                unique: true);

            // «Склад тканин» moves from «Швейний цех»: the rows of the production schema are copied with their ids, so
            // the order cards and the old-app ids keep pointing at them, and the old tables are dropped in the same
            // transaction (Production's MoveFabricStockToWarehouse then has nothing to drop). A new database has nothing
            // to copy.
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF to_regclass('production.""FabricStocks""') IS NOT NULL THEN
        INSERT INTO warehouse.""FabricStocks"" (""Id"", ""FabricId"", ""FabricCode"", ""FabricName"", ""Color"", ""ColorKey"", ""Meters"", ""HasMovements"", ""CreatedAt"", ""UpdatedAt"", ""Version"", ""LegacyId"")
        SELECT ""Id"", ""FabricId"", ""FabricCode"", ""FabricName"", ""Color"", ""ColorKey"", ""Meters"", ""HasMovements"", ""CreatedAt"", ""UpdatedAt"", ""Version"", ""LegacyId"" FROM production.""FabricStocks"" ORDER BY ""Id"";
        INSERT INTO warehouse.""FabricMovements"" (""Id"", ""StockId"", ""Kind"", ""MetersBefore"", ""Delta"", ""MetersAfter"", ""OperationDate"", ""Note"", ""SupplierId"", ""SupplierName"", ""Document"", ""OrderId"", ""ItemId"", ""OrderNumber"", ""ModelName"", ""ReversesMovementId"", ""IsReversed"", ""CreatedByName"", ""CreatedAt"", ""LegacyId"")
        SELECT ""Id"", ""StockId"", ""Kind"", ""MetersBefore"", ""Delta"", ""MetersAfter"", ""OperationDate"", ""Note"", ""SupplierId"", ""SupplierName"", ""Document"", ""OrderId"", ""ItemId"", ""OrderNumber"", ""ModelName"", ""ReversesMovementId"", ""IsReversed"", ""CreatedByName"", ""CreatedAt"", ""LegacyId"" FROM production.""FabricMovements"" ORDER BY ""Id"";
        INSERT INTO warehouse.""FabricIssues"" (""Id"", ""OrderId"", ""ItemId"", ""OrderNumber"", ""ModelName"", ""IssuedOn"", ""Note"", ""CreatedByName"", ""CreatedAt"", ""ReversedAt"", ""ReversedByName"", ""ReverseReason"", ""LegacyId"")
        SELECT ""Id"", ""OrderId"", ""ItemId"", ""OrderNumber"", ""ModelName"", ""IssuedOn"", ""Note"", ""CreatedByName"", ""CreatedAt"", ""ReversedAt"", ""ReversedByName"", ""ReverseReason"", ""LegacyId"" FROM production.""FabricIssues"" ORDER BY ""Id"";
        INSERT INTO warehouse.""FabricIssueLines"" (""Id"", ""IssueId"", ""Purpose"", ""PlanFabricCode"", ""PlanColorKey"", ""PlannedMeters"", ""StockId"", ""FabricCode"", ""FabricName"", ""Color"", ""Meters"", ""CoveredMeters"", ""MovementId"")
        SELECT ""Id"", ""IssueId"", ""Purpose"", ""PlanFabricCode"", ""PlanColorKey"", ""PlannedMeters"", ""StockId"", ""FabricCode"", ""FabricName"", ""Color"", ""Meters"", ""CoveredMeters"", ""MovementId"" FROM production.""FabricIssueLines"" ORDER BY ""Id"";
        PERFORM setval(pg_get_serial_sequence('warehouse.""FabricStocks""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM warehouse.""FabricStocks""), 0) + 1, false);
        PERFORM setval(pg_get_serial_sequence('warehouse.""FabricMovements""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM warehouse.""FabricMovements""), 0) + 1, false);
        PERFORM setval(pg_get_serial_sequence('warehouse.""FabricIssues""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM warehouse.""FabricIssues""), 0) + 1, false);
        PERFORM setval(pg_get_serial_sequence('warehouse.""FabricIssueLines""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM warehouse.""FabricIssueLines""), 0) + 1, false);
        DROP TABLE production.""FabricIssueLines"";
        DROP TABLE production.""FabricIssues"";
        DROP TABLE production.""FabricMovements"";
        DROP TABLE production.""FabricStocks"";
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FabricIssueLines",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "FabricIssues",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "FabricMovements",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "FabricStocks",
                schema: "warehouse");
        }
    }
}
