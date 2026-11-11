using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveFabricStockToWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // «Склад тканин» now lives in the Warehouse module (schema warehouse). Its first migration copies these rows
            // and drops the tables; when it ran first there is nothing left here. Before it ran, the rows stay for it.
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF to_regclass('warehouse.""FabricStocks""') IS NOT NULL THEN
        DROP TABLE IF EXISTS production.""FabricIssueLines"";
        DROP TABLE IF EXISTS production.""FabricIssues"";
        DROP TABLE IF EXISTS production.""FabricMovements"";
        DROP TABLE IF EXISTS production.""FabricStocks"";
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FabricIssues",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModelName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false),
                    ReverseReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReversedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReversedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricIssues_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "production",
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricIssues_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "production",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FabricStocks",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Color = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ColorKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FabricId = table.Column<int>(type: "integer", nullable: true),
                    FabricName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    HasMovements = table.Column<bool>(type: "boolean", nullable: false),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Meters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricStocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricMovements",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Delta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Document = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MetersAfter = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MetersBefore = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ModelName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OperationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    OrderNumber = table.Column<int>(type: "integer", nullable: true),
                    ReversesMovementId = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricMovements_FabricMovements_ReversesMovementId",
                        column: x => x.ReversesMovementId,
                        principalSchema: "production",
                        principalTable: "FabricMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricMovements_FabricStocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "production",
                        principalTable: "FabricStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FabricIssueLines",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovementId = table.Column<int>(type: "integer", nullable: false),
                    StockId = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CoveredMeters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    FabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FabricName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IssueId = table.Column<int>(type: "integer", nullable: false),
                    Meters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    PlanColorKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PlanFabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PlannedMeters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Purpose = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricIssueLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricIssueLines_FabricIssues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "production",
                        principalTable: "FabricIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FabricIssueLines_FabricMovements_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "production",
                        principalTable: "FabricMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricIssueLines_FabricStocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "production",
                        principalTable: "FabricStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssueLines_IssueId",
                schema: "production",
                table: "FabricIssueLines",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssueLines_MovementId",
                schema: "production",
                table: "FabricIssueLines",
                column: "MovementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssueLines_StockId",
                schema: "production",
                table: "FabricIssueLines",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssues_ItemId",
                schema: "production",
                table: "FabricIssues",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssues_LegacyId",
                schema: "production",
                table: "FabricIssues",
                column: "LegacyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssues_OrderId",
                schema: "production",
                table: "FabricIssues",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_ItemId",
                schema: "production",
                table: "FabricMovements",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_LegacyId",
                schema: "production",
                table: "FabricMovements",
                column: "LegacyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_OperationDate",
                schema: "production",
                table: "FabricMovements",
                column: "OperationDate");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_ReversesMovementId",
                schema: "production",
                table: "FabricMovements",
                column: "ReversesMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_StockId_Id",
                schema: "production",
                table: "FabricMovements",
                columns: new[] { "StockId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_FabricStocks_FabricCode_ColorKey",
                schema: "production",
                table: "FabricStocks",
                columns: new[] { "FabricCode", "ColorKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricStocks_LegacyId",
                schema: "production",
                table: "FabricStocks",
                column: "LegacyId",
                unique: true);
        }
    }
}
