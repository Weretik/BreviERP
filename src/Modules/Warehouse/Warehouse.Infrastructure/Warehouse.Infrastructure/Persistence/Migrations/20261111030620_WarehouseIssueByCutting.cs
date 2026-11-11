using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseIssueByCutting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IssueLineId",
                schema: "warehouse",
                table: "FabricMovements",
                type: "integer",
                nullable: true);

            // A line of an issue may now spend a pair over several stock lines: the link moves to the movements.
            migrationBuilder.Sql("""
                UPDATE warehouse."FabricMovements" AS m
                SET "IssueLineId" = l."Id"
                FROM warehouse."FabricIssueLines" AS l
                WHERE l."MovementId" = m."Id";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_FabricIssueLines_FabricMovements_MovementId",
                schema: "warehouse",
                table: "FabricIssueLines");

            migrationBuilder.DropIndex(
                name: "IX_FabricIssueLines_MovementId",
                schema: "warehouse",
                table: "FabricIssueLines");

            migrationBuilder.DropColumn(
                name: "MovementId",
                schema: "warehouse",
                table: "FabricIssueLines");

            migrationBuilder.AddColumn<int>(
                name: "IssuedQuantity",
                schema: "warehouse",
                table: "FabricIssues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                schema: "warehouse",
                table: "FabricIssues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanVersion",
                schema: "warehouse",
                table: "FabricIssues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Layer",
                schema: "warehouse",
                table: "FabricIssueLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "warehouse",
                table: "FabricIssueLines",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanCoverage",
                schema: "warehouse",
                table: "FabricIssueLines",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE warehouse."FabricIssueLines" SET "PlanCoverage" = "CoveredMeters";
                """);

            migrationBuilder.CreateTable(
                name: "FabricItemPlans",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricItemPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricItemPlanParts",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Color = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ColorKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Layer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Meters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricItemPlanParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricItemPlanParts_FabricItemPlans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "warehouse",
                        principalTable: "FabricItemPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_IssueLineId",
                schema: "warehouse",
                table: "FabricMovements",
                column: "IssueLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssues_OperationId",
                schema: "warehouse",
                table: "FabricIssues",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricItemPlanParts_PlanId",
                schema: "warehouse",
                table: "FabricItemPlanParts",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricItemPlans_ItemId",
                schema: "warehouse",
                table: "FabricItemPlans",
                column: "ItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FabricMovements_FabricIssueLines_IssueLineId",
                schema: "warehouse",
                table: "FabricMovements",
                column: "IssueLineId",
                principalSchema: "warehouse",
                principalTable: "FabricIssueLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FabricMovements_FabricIssueLines_IssueLineId",
                schema: "warehouse",
                table: "FabricMovements");

            migrationBuilder.DropTable(
                name: "FabricItemPlanParts",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "FabricItemPlans",
                schema: "warehouse");

            migrationBuilder.DropIndex(
                name: "IX_FabricMovements_IssueLineId",
                schema: "warehouse",
                table: "FabricMovements");

            migrationBuilder.DropIndex(
                name: "IX_FabricIssues_OperationId",
                schema: "warehouse",
                table: "FabricIssues");

            migrationBuilder.DropColumn(
                name: "IssuedQuantity",
                schema: "warehouse",
                table: "FabricIssues");

            migrationBuilder.DropColumn(
                name: "OperationId",
                schema: "warehouse",
                table: "FabricIssues");

            migrationBuilder.DropColumn(
                name: "PlanVersion",
                schema: "warehouse",
                table: "FabricIssues");

            migrationBuilder.DropColumn(
                name: "Layer",
                schema: "warehouse",
                table: "FabricIssueLines");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "warehouse",
                table: "FabricIssueLines");

            migrationBuilder.DropColumn(
                name: "PlanCoverage",
                schema: "warehouse",
                table: "FabricIssueLines");

            migrationBuilder.AddColumn<int>(
                name: "MovementId",
                schema: "warehouse",
                table: "FabricIssueLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE warehouse."FabricIssueLines" AS l
                SET "MovementId" = (SELECT MIN(m."Id") FROM warehouse."FabricMovements" AS m WHERE m."IssueLineId" = l."Id");
                """);

            migrationBuilder.DropColumn(
                name: "IssueLineId",
                schema: "warehouse",
                table: "FabricMovements");

            migrationBuilder.CreateIndex(
                name: "IX_FabricIssueLines_MovementId",
                schema: "warehouse",
                table: "FabricIssueLines",
                column: "MovementId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FabricIssueLines_FabricMovements_MovementId",
                schema: "warehouse",
                table: "FabricIssueLines",
                column: "MovementId",
                principalSchema: "warehouse",
                principalTable: "FabricMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
