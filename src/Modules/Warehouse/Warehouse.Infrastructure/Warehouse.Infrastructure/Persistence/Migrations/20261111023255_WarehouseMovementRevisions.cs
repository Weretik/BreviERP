using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseMovementRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                schema: "warehouse",
                table: "FabricMovements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                schema: "warehouse",
                table: "FabricMovements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FabricMovementRevisions",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovementId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BeforeStockId = table.Column<int>(type: "integer", nullable: false),
                    BeforeOperationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BeforeDelta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    BeforeSupplierName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    BeforeDocument = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BeforeDestination = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    BeforeIsVoided = table.Column<bool>(type: "boolean", nullable: false),
                    AfterStockId = table.Column<int>(type: "integer", nullable: false),
                    AfterOperationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AfterDelta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    AfterSupplierName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AfterDocument = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AfterDestination = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    AfterIsVoided = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RestoresRevisionId = table.Column<int>(type: "integer", nullable: true),
                    IsUndone = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricMovementRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricMovementRevisions_FabricMovementRevisions_RestoresRev~",
                        column: x => x.RestoresRevisionId,
                        principalSchema: "warehouse",
                        principalTable: "FabricMovementRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricMovementRevisions_FabricMovements_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "warehouse",
                        principalTable: "FabricMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricMovementRevisions_FabricStocks_AfterStockId",
                        column: x => x.AfterStockId,
                        principalSchema: "warehouse",
                        principalTable: "FabricStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricMovementRevisions_FabricStocks_BeforeStockId",
                        column: x => x.BeforeStockId,
                        principalSchema: "warehouse",
                        principalTable: "FabricStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovementRevisions_AfterStockId",
                schema: "warehouse",
                table: "FabricMovementRevisions",
                column: "AfterStockId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovementRevisions_BeforeStockId",
                schema: "warehouse",
                table: "FabricMovementRevisions",
                column: "BeforeStockId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovementRevisions_MovementId_Number",
                schema: "warehouse",
                table: "FabricMovementRevisions",
                columns: new[] { "MovementId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovementRevisions_RestoresRevisionId",
                schema: "warehouse",
                table: "FabricMovementRevisions",
                column: "RestoresRevisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FabricMovementRevisions",
                schema: "warehouse");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                schema: "warehouse",
                table: "FabricMovements");

            migrationBuilder.DropColumn(
                name: "Revision",
                schema: "warehouse",
                table: "FabricMovements");
        }
    }
}
